using Expense.Tracker.Data;
using ExpenseTracker.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Expense.Tracker.Security
{
    public class AuthorizeRolesAttribute : AuthorizeAttribute
    {
        private bool IsAuthorized;
        public AuthorizeRolesAttribute()
            : base()
        {
            this.IsAuthorized = false;
        }

        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            if (!httpContext.User.Identity.IsAuthenticated)
                this.IsAuthorized = false;
            else
            {
                //Check roles
                string userName = Web.GetUserName(httpContext);
                Web.SessionUser = userName;

                string connection = ConfigurationManager.AppSettings["DBConnectionString"];
                DataManager manager = null;
                if (httpContext.Application["connectionManager"] == null)
                {
                    manager = new DataManager(connection);
                    httpContext.Application["connectionManager"] = manager;
                }

                else
                    manager = (DataManager)httpContext.Application["connectionManager"];
                //check for SuperAdmin
                SecurityHandler handler = new SecurityHandler(userName, httpContext, manager);
                if (handler.HasSuperAdminToken(userName))
                    return true;

                //current controller
                var controller = Web.GetControllerName(httpContext);
                var action = Web.GetActionName(httpContext);
                if (string.IsNullOrEmpty(controller))
                {
                    this.IsAuthorized = false;
                    return this.IsAuthorized;
                }
                this.IsAuthorized = handler.HasRequiredToken(controller, action);
            }
            return this.IsAuthorized;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                //Here login page is called
                base.HandleUnauthorizedRequest(filterContext);
            }
            else
            {
                if (!this.IsAuthorized)
                {
                    var tempData = new TempDataDictionary();
                    tempData.Add("ErrorReason", "Unauthorized");
                    filterContext.Result = new ViewResult() { ViewName = "Error", TempData = tempData };
                }
            }
        }
    }
}
