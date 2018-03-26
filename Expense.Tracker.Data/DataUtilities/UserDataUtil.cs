using Expense.Tracker.Data.DataModels;
using Expense.Tracker.Data.DataUtilities.Base;
using Expense.Tracker.Data.EntityModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense.Tracker.Data.DataUtilities
{
    public class UserDataUtil : BaseDataUtils
    {
        public UserDataUtil(DataManager bridge)
            : base(bridge)
        {
            
        }

        public bool DoesSuperAdminTokenExists(string userName, string superAdminTokenName)
        {
            bool hasData = false;
            try
            {
                string sql = @"select R.* from Users AU  
	                        INNER JOIN UserRole orl on orl.UserId = AU.UserId 
	                        Inner Join Role r on orl.RoleId = r.RoleId and IsEditAllowed = 0
	                        where R.RoleName = @p0
	                        and AU.UserEmail = @p1";
                hasData = this.DbFactory.Roles.SqlQuery(sql, superAdminTokenName, userName).Count() >= 1;

            }
            catch { }
            return hasData;
        }

        /// <summary>
        /// Checks whether the current feature is supported for the User.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="currentfeature"></param>
        /// <returns></returns>
        public bool DoesFeatureExists(string userId, Guid featureId)
        {
            bool hasData = false;
            try
            {
                string sql = @"select F.* from Users AU 
                            INNER JOIN UserRole OU on OU.UserId = AU.UserId 
                            INNER JOIN Role orl on orl.RoleId = OU.RoleId 
                            INNER JOIN RoleFeature RF on RF.RoleId = orl.RoleId 
                            INNER JOIN Feature F on F.FeatureId = RF.FeatureId and ISNULL(F.ACTIVE, 1) = 1 AND F.Location=0 
                            where AU.UserEmail = @p0 AND F.FeatureId = @p1";
                hasData = this.DbFactory.Features.SqlQuery(sql, userId, featureId).Count() >= 1;

            }
            catch { }
            return hasData;
        }

        public RoleFeatureModel GetRoleFeature(Guid roleId)
        {
            var role = this.DbFactory.Roles.Find(roleId);

            return new RoleFeatureModel(role);
        }

        public IEnumerable<Feature> GetAllFeatures()
        {
            return this.DbFactory.Features;
        }

        public ReturnMessage<User> GenerateForgetPassword(string emailId)
        {
            ReturnMessage<User> returnMessage = new ReturnMessage<User>("Account does not exists in server. Please ensure you put a valid email id.", false);
            try
            {


                var appusers = this.DbFactory.Users.Where(u => u.UserEmail == emailId);
                if (appusers != null && appusers.Any())
                {
                    User appuser = appusers.FirstOrDefault();
                    if (appuser.IsActive == false || appuser.IsActive == null)
                    {
                        returnMessage.Message = "Account is not active. Please check your mailbox for activation link";
                        return returnMessage;
                    }
                    appuser.ForgetPasswordCode = Guid.NewGuid();
                    this.DbFactory.Entry(appuser).State = EntityState.Modified;
                    this.DbFactory.SaveChanges();

                    returnMessage.Value = appuser;
                    returnMessage.Status = true;
                }
            }
            catch
            {
                returnMessage.Status = false;
            }

            return returnMessage;
        }
    }
}
