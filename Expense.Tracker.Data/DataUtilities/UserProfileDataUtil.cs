using Expense.Tracker.Data.DataModels;
using Expense.Tracker.Data.DataUtilities.Base;
using Expense.Tracker.Data.EntityModel;
using ExpenseTracker.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Expense.Tracker.Data.DataUtilities
{
    public class UserProfileDataUtils : BaseDataUtils
    {
        private ExpenseTrackerEntities db = null;

     
        public UserProfileDataUtils(string userName, DataManager bridge)
            : base(bridge)
        {
            this.db = bridge.DataFactory;
            this.UserName = userName;
        }

        public ProfileModel GetProfile(HttpContextBase currentContext)
        {
            string userName = Web.GetUserName(currentContext);
            var appUser = this.AppUser;
            

            ProfileModel pModel = new ProfileModel
            {
                CurrentUser = appUser
            };
            return pModel;
        }

        private User _currentAppUser;
        public User AppUser
        {
            get
            {
                if (this._currentAppUser == null)
                    this._currentAppUser = this.db.Users.FirstOrDefault(e => e.UserEmail.Equals(this.UserName));
                return this._currentAppUser;
            }
        }
        public string UserName { get; set; }

        public Role GetCurrentRole()
        {
            var userrole = this.db.UserRoles.FirstOrDefault(e => e.UserId == this.AppUser.UserId);
            return userrole.Role;
        }

        public bool ChangeUserPassword(string oldPassword, string newPassword)
        {
            return this.ChangeUserPassword(this.AppUser, oldPassword, newPassword);
        }
        public bool ChangeUserPassword(User user, string oldPassword, string newPassword)
        {
            try
            {
                var appUser = this.DbFactory.Users.Find(user.UserId);
                string eoldPassword = CryptUtils.GetPasswordEncrypted(oldPassword);
                if (eoldPassword.Equals(appUser.UserPassword))
                {
                    string enewPassword = CryptUtils.GetPasswordEncrypted(newPassword);
                    appUser.UserPassword = enewPassword;
                    this.db.Entry(appUser).State = System.Data.Entity.EntityState.Modified;
                    this.db.SaveChanges();
                    return true;
                }

            }
            catch { }
            return false;
        }
        public bool EditCurrentDetails(string fullName, string contactNumber)
        {
            return this.EditCurrentDetails(this.AppUser, fullName, contactNumber, null);
        }

        public bool EditCurrentDetails(User user, string fullName, string contactNumber, bool? isSubscribed)
        {
            try
            {
                var appUser = this.DbFactory.Users.Find(user.UserId);
                if (!string.IsNullOrWhiteSpace(fullName)) // you cannot reset name to blank
                {
                    appUser.UserFullName = fullName;
                }

                appUser.UserContactNumber = contactNumber;
 

                this.db.Entry(appUser).State = System.Data.Entity.EntityState.Modified;
                this.db.SaveChanges();
                return true;
            }
            catch { }
            return false;
        }
        public string InvokeForgotPasswordOperation()
        {
            string transactionid = string.Empty;
            try
            {
                Guid forgetpasswordGuid = Guid.NewGuid();
                //We pass last 12 characters of the guid
                string guid = forgetpasswordGuid.ToString();
                transactionid = guid.Substring(guid.LastIndexOf('-') + 1);

                this.AppUser.ForgetPasswordCode = forgetpasswordGuid;

                this.db.SaveChanges();
            }
            catch { }

            return transactionid;
        }
        public IEnumerable<Configuration> GetConfiguration(Guid orgId)
        {
            var configurations = db.Configurations.Where(c => (c.ParentEntity.Equals("Org") && c.ParentId == orgId) || c.ParentEntity.Equals("Global"));

            List<string> lstStrings = new List<string>();
            foreach (var config in configurations.Where(e => e.ParentEntity.Equals("Org")))
            {
                lstStrings.Add(config.ConfigKey);
                yield return config;
            }

            foreach (var config in configurations.Where(e => e.ParentEntity.Equals("Global")))
            {
                if (!lstStrings.Any(e => e.Equals(config.ConfigKey)))
                {
                    lstStrings.Add(config.ConfigKey);
                    yield return config;
                }
            }
        }
      
    }
}
