using Expense.Tracker.Data.EntityModel;
using Expense.Tracker.Security;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace Expense.Tracker.Web.Models
{
    public interface IFormsAuthentication
    {
        void SignIn(string userName, bool createPersistentCookie);
        void SignOut();
    }
    public class FormsAuthenticationService : IFormsAuthentication
    {
        private readonly ExpenseTrackerEntities _db = null;

        public FormsAuthenticationService(ExpenseTrackerEntities db)
        {
            this._db = db;
        }
        public string SignedInUsername
        {
            get { return HttpContext.Current.User.Identity.Name; }
        }

        public DateTime? SignedInTimestampUtc
        {
            get
            {
                var cookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                if (cookie != null)
                {
                    var ticket = FormsAuthentication.Decrypt(cookie.Value);
                    return ticket.IssueDate.ToUniversalTime();
                }
                else
                {
                    return null;
                }
            }
        }

        public void SignIn(string userName, bool createPersistentCookie)
        {
            FormsAuthentication.SetAuthCookie(userName, createPersistentCookie);
            User user = _db.Users.FirstOrDefault(c => c.UserEmail == userName);
            if (user != null)
            {  

                var roleId = this.GetRole(user.UserId); 
                HttpContext.Current.Session[Constants.CON_ROLE] = roleId;
            }
        }

        private Guid GetRole(Guid UserId)
        {

            string sql = @"select RoleId from UserRole 
                        Inner Join Users on Users.UserId = UserRole.UserId
                        where Users.UserId = @p0";

            var result = this._db.Database.SqlQuery<Guid>(sql, UserId).FirstOrDefault();

            return result;
        }

        public void SignOut()
        {
            FormsAuthentication.SignOut();
            HttpContext.Current.Session.Abandon();
        }
    }
}