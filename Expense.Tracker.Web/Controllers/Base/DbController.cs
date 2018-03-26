using Expense.Tracker.Data;
using Expense.Tracker.Data.DataUtilities;
using Expense.Tracker.Data.EntityModel;
using Expense.Tracker.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Expense.Tracker.Web.Controllers.Base
{
    public class DbController : Controller
    {
        DataManager _databaseFactory;

        /// <summary>
        /// Gets the database factory.
        /// </summary>
        /// <value>
        /// The database factory.
        /// </value>
        public DataManager DatabaseFactory
        {
            get
            {
                if (this._databaseFactory == null)
                {
                    string connection = ConfigurationManager.AppSettings["DBConnectionString"];
                    this._databaseFactory = new DataManager(connection);
                }
                return this._databaseFactory;
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="DbControllerBase"/> class.
        /// </summary>
        public DbController()
            : base()
        {

        }

        /// <summary>
        /// Gets the data bridge.
        /// </summary>
        /// <value>
        /// The data bridge.
        /// </value>
        public ExpenseTrackerEntities DataBridge
        {
            get
            {
                return this.DatabaseFactory.DataFactory;
            }
        }


        /// <summary>
        /// Called after the action method is invoked.
        /// </summary>
        /// <param name="filterContext">Information about the current request and action.</param>
        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {

            base.OnActionExecuted(filterContext);
            this.SetFeaturesBag();
            var controller = ExpenseTracker.Utilities.Web.GetControllerName(this.HttpContext);
            var action = ExpenseTracker.Utilities.Web.GetActionName(this.HttpContext);
            ViewBag.CurrentController = controller;
            ViewBag.CurrentAction = action;
        }
        ///// <summary>
        ///// Gets all features.
        ///// </summary>
        ///// <returns></returns>
        //protected IEnumerable<Feature> GetAllFeatures()
        //{
        //    AuthorizationDataUtils authData = this.DatabaseFactory.AuthUtils;
        //    string userName = ExpenseTracker.Utilities.Web.GetUserName(this.HttpContext);

        //    return authData.GetAllFeatures(userName);

        //}

        ///// <summary>
        ///// Determines whether the specified controller has feature.
        ///// </summary>
        ///// <param name="controller">The controller.</param>
        ///// <param name="action">The action.</param>
        ///// <returns></returns>
        //public bool HasFeature(string controller, string action)
        //{
        //    // Should be using Cache to detect a feature.
        //    string userName = ExpenseTracker.Utilities.Web.GetUserName(this.HttpContext);
        //    var feature = this.DatabaseFactory.AuthUtils.CheckParticularFeature(controller, action, userName);

        //    if (feature != null)
        //    {
        //        if (!string.IsNullOrWhiteSpace(feature.HelpUrl))
        //            ViewBag.CurrentHelpUrl = Path.Combine("http://support.appseconnect.com/support/solutions", feature.HelpUrl);
        //        return true;
        //    }
        //    return false;
        //}

        /// <summary>
        /// Gets all root features.
        /// </summary>
        /// <returns></returns>
        protected IEnumerable<Feature> GetAllRootFeatures()
        {
            var features = this.GetFeatureBag();
            //this.SetFeaturesBag(features);
            return features;
        }
        /// <summary>
        /// Gets the current role.
        /// </summary>
        /// <returns></returns>
        public Role GetCurrentRole()
        {
            var roleId = this.HttpContext.Session[Constants.CON_ROLE];

            if (roleId == null)
            {
                //Resolve role
                try
                {
                    string email = ExpenseTracker.Utilities.Web.GetUserName(this.HttpContext);
                    var role = this.DatabaseFactory.UserProfileUtils.GetCurrentRole();
                    this.HttpContext.Session[Constants.CON_ROLE] = role.RoleId;
                    return role;
                }
                catch
                {

                }
            }

            return this.DataBridge.Roles.Find(roleId);

        }
        /// <summary>
        /// Gets the feature bag.
        /// </summary>
        /// <returns></returns>
        public List<Feature> GetFeatureBag()
        {

            List<Feature> features = this.HttpContext.Application["Features"] as List<Feature>;
            if (features == null)
            {
                AuthorizationDataUtils authData = this.DatabaseFactory.AuthUtils;
                string userName = ExpenseTracker.Utilities.Web.GetUserName(this.HttpContext);
                features = authData.GetAllRootFeatures(userName).OrderBy(e => e.ORDERING).ToList();
                this.SetToApplication(features);
            }
            return features;
        }

        private void SetToApplication(List<Feature> features)
        {
            this.HttpContext.Application.Lock();
            this.HttpContext.Application["Features"] = features;
            this.HttpContext.Application.UnLock();
        }

        internal void ClearApplicationFeatures()
        {
            this.HttpContext.Application.Lock();
            this.HttpContext.Application["Features"] = null;
            this.HttpContext.Application.UnLock();
        }
        /// <summary>
        /// Sets the features bag.
        /// </summary>
        protected void SetFeaturesBag()
        {
            if (this.HttpContext != null && this.HttpContext.User.Identity.IsAuthenticated)
            {
                this.SetMenuBag(this.GetMenuBag());
                this.SetFeaturesBag(this.GetFeatureBag());
            }
        }

        private void SetMenuBag(List<Feature> list)
        {
            ViewBag.Menu = list;
        }

        private List<Feature> GetMenuBag()
        {
            var roleSession = this.HttpContext.Session[Constants.CON_ROLE];
            if (roleSession == null)
            {
                var role = this.GetCurrentRole();
                if (role != null)
                    roleSession = role.RoleId;
            }
            if (roleSession != null)
            {
                var roleId = Guid.Parse(roleSession.ToString());
                List<Feature> features = this.HttpContext.Session[roleId.ToString()] as List<Feature>;
                if (features == null)
                {
                    AuthorizationDataUtils authData = this.DatabaseFactory.AuthUtils;
                    var appUser = this.DatabaseFactory.UserProfileUtils.AppUser;
                    if (appUser != null)
                    {
                        features = authData.GetMenu(appUser.UserId, roleId).OrderBy(e => e.ORDERING).ToList();
                        this.Session[roleId.ToString()] = features;
                    }
                }
                return features;
            }
            return null;
        }

        /// <summary>
        /// Sets the features bag.
        /// </summary>
        /// <param name="features">The features.</param>
        protected void SetFeaturesBag(List<Feature> features)
        {
            var featurelst = features;
            var appUser = this.DatabaseFactory.UserProfileUtils.AppUser;
            this.ViewBag.FeatureModel = featurelst;

            this.ViewBag.UserFullName = appUser.UserFullName;
            this.ViewBag.ProfilePic = appUser.ProfilePic;
            var controller = ExpenseTracker.Utilities.Web.GetControllerName(this.HttpContext);
            var action = ExpenseTracker.Utilities.Web.GetActionName(this.HttpContext);
            if (action == "Index")
                action = string.Empty;

            var feature = featurelst.FirstOrDefault(e => e.Controller == controller && e.Action == action);
            ViewBag.CurrentHelpUrl = null;
            if (feature != null)
            {
                ViewBag.CurrentHelpUrl = "#";
            }
        }
        /// <summary>
        /// Determines whether [is valid unique identifier] [the specified unique identifier].
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <returns></returns>
        protected bool IsValidGuid(Guid guid)
        {
            return guid != Guid.Parse("00000000-0000-0000-0000-000000000000");
        }

 
    }
}