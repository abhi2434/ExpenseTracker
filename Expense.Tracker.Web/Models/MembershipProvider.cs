using Expense.Tracker.Data.EntityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace Expense.Tracker.Web.Models
{
 
    //for conntroller and formAuth
    /// <summary>
    /// ExpenseTracker Admin Membership provider
    /// </summary>
    public class ExpenseTrackerCloudMembershipProvider : MembershipProvider
    {
        public ExpenseTrackerCloudMembershipProvider()
        {
        }
        public override bool ValidateUser(string username, string password)
        {
            using (ExpenseTrackerEntities _db = new ExpenseTrackerEntities())
            {
                try
                {
                    var user = _db.Users.FirstOrDefault(u => u.UserEmail == username);
                    user.UserPassword = password;
                    _db.SaveChanges();
                    if (user != null)
                    {
                        //Update on every login validation success
                        this.LogUserLogin(user, _db, HttpContext.Current);
                        return true;
                    }
                }
                catch(Exception ex)
                {
                }
                return false;
            }
        }

        public void LogUserLogin(User currentUser, ExpenseTrackerEntities _db, HttpContext currentContext)
        {
            //currentUser.LastLoggedIn = DateTime.Now;
            currentUser.LAST_ACCESS_IP = ExpenseTracker.Utilities.Web.GetClientIPAddress(currentContext);

            _db.SaveChanges();
        }

        #region NotImplemented Methods & Properties
        public override string ApplicationName
        {
            get
            {
                return this.ApplicationName;
            }
            set
            {
                this.ApplicationName = value;
            }
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            throw new System.NotImplementedException();
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new System.NotImplementedException();
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            throw new System.NotImplementedException();
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            throw new System.NotImplementedException();
        }

        public override bool EnablePasswordReset
        {
            get { return true; }
        }

        public override bool EnablePasswordRetrieval
        {
            get { return true; }
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new System.NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new System.NotImplementedException();
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new System.NotImplementedException();
        }

        public override int GetNumberOfUsersOnline()
        {
            throw new System.NotImplementedException();
        }

        public override string GetPassword(string username, string answer)
        {
            throw new System.NotImplementedException();
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            throw new System.NotImplementedException();
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            throw new System.NotImplementedException();
        }

        public override string GetUserNameByEmail(string email)
        {
            throw new System.NotImplementedException();
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { throw new System.NotImplementedException(); }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { throw new System.NotImplementedException(); }
        }

        public override int MinRequiredPasswordLength
        {
            get { return 6; }
        }

        public override int PasswordAttemptWindow
        {
            get { throw new System.NotImplementedException(); }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { throw new System.NotImplementedException(); }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { throw new System.NotImplementedException(); }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { throw new System.NotImplementedException(); }
        }

        public override bool RequiresUniqueEmail
        {
            get { throw new System.NotImplementedException(); }
        }

        public override string ResetPassword(string username, string answer)
        {
            throw new System.NotImplementedException();
        }

        public override bool UnlockUser(string userName)
        {
            throw new System.NotImplementedException();
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}