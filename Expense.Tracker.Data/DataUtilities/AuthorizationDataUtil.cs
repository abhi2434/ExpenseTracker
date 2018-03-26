using Expense.Tracker.Data.DataModels;
using Expense.Tracker.Data.DataUtilities.Base;
using Expense.Tracker.Data.EntityModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense.Tracker.Data.DataUtilities
{
    public class AuthorizationDataUtils : BaseDataUtils
    {
        private ExpenseTrackerEntities db = null;

       
        public AuthorizationDataUtils(DataManager bridge)
            : base(bridge)
        {
            db = bridge.DataFactory;
        }

        public IEnumerable<Feature> GetAllFeatures()
        {
            return this.db.Features;
        }

        /// <summary>
        /// Gets all the Leaf node features where the user has permission to
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IEnumerable<Feature> GetAllFeatures(string userId)
        {
            string sql = @"select F.* from AppUser AU 
                            INNER JOIN OrgUser OU on OU.AppUserId = AU.AppUserId AND OU.IsDefaultOrg = 1
                            INNER JOIN OrgRole orl on orl.OrgId = OU.OrgId 
                            INNER JOIN OrgUserRole our on our.OrgRoleId = orl.OrgRoleId
                            INNER JOIN RoleFeature RF on RF.RoleId = orl.RoleId 
                            INNER JOIN Feature F on F.FeatureId = RF.FeatureId and ISNULL(F.ACTIVE, 1) = 1 AND F.Location=0 
                            where AU.AppUserEmail = @p0";
            var features = db.Features.SqlQuery(sql, userId);

            return features.AsEnumerable();
        }
        public IEnumerable<FeatureModel> GetAllFeaturesSerialized(string userId)
        {
            var features = this.GetAllFeatures(userId);
            foreach (var feature in features)
                yield return new FeatureModel(feature);
        }
        public Feature CheckParticularFeature(string controller, string action, string userId)
        {
            string sql = @"select F.* from AppUser AU 
                            INNER JOIN OrgUser OU on OU.AppUserId = AU.AppUserId AND OU.IsDefaultOrg = 1
                            INNER JOIN OrgRole orl on orl.OrgId = OU.OrgId 
                            INNER JOIN OrgUserRole our on our.OrgRoleId = orl.OrgRoleId
                            INNER JOIN RoleFeature RF on RF.RoleId = orl.RoleId 
                            INNER JOIN Feature F on F.FeatureId = RF.FeatureId and ISNULL(F.ACTIVE, 1) = 1 AND F.Location=0 
                            where AU.AppUserEmail = @p0
                            and F.Controller = @p1 and F.Action=@p2";
            return db.Features.SqlQuery(sql, userId, controller, action).FirstOrDefault();

        }

       
        public IEnumerable<Feature> GetAllRootFeatures(string userId)
        {
            //            string sql = @"select distinct F.* from AppUser AU 
            //            INNER JOIN OrgUser OU on OU.AppUserId = AU.AppUserId AND OU.IsDefaultOrg = 1
            //            INNER JOIN OrgRole orl on orl.OrgId = OU.OrgId 
            //            INNER JOIN OrgUserRole our on our.OrgRoleId = orl.OrgRoleId
            //            INNER JOIN RoleFeature RF on RF.RoleId = orl.RoleId 
            //            INNER JOIN Feature F on F.FeatureId = RF.FeatureId and ISNULL(F.ACTIVE, 1) = 1 AND F.Location=0 
            //                    AND PARENTFEATUREID IS NULL AND ISNULL(F.Ordering, 0) >= 0 AND IsNull(IsListing, 1) = 1
            //            where AU.AppUserEmail = '" + userId + "' Order by F.Ordering, F.FeatureName";

            string sql = @"select distinct F.* from Feature F 
                        Inner join RoleFeature RF on RF.FeatureId = F.FeatureId 
							                        And ISNULL(F.ACTIVE, 1) = 1  
							                        AND PARENTFEATUREID IS NULL 
							                        AND ISNULL(F.Ordering, 0) >= 0 
							                        AND IsNull(IsListing, 1) = 1
                        Inner join UserRole OG on OG.RoleId = RF.RoleId 
                        Inner join Users AU on AU.UserId = OG.UserId
                        where AU.UserEmail = @p0
                        order by F.Ordering, F.FeatureName";

            var features = db.Features.SqlQuery(sql, new SqlParameter("p0", userId));
            return features.AsEnumerable();
        }
        public IEnumerable<FeatureModel> GetAllRootFeaturesSerialized(string userId)
        {
            var features = this.GetAllRootFeatures(userId);
            foreach (var feature in features)
                yield return new FeatureModel(feature);
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
                string sql = @"select F.* from AppUser AU 
                            INNER JOIN OrgUser OU on OU.AppUserId = AU.AppUserId AND OU.IsDefaultOrg = 1
                            INNER JOIN OrgRole orl on orl.OrgId = OU.OrgId 
                            INNER JOIN OrgUserRole our on our.OrgRoleId = orl.OrgRoleId
                            INNER JOIN RoleFeature RF on RF.RoleId = orl.RoleId 
                            INNER JOIN Feature F on F.FeatureId = RF.FeatureId and ISNULL(F.ACTIVE, 1) = 1 AND F.Location=0 
                            where AU.AppUserEmail = @p0 AND F.FeatureId = @p1";
                hasData = db.Features.SqlQuery(sql, userId, featureId).Count() >= 1;

            }
            catch { }
            return hasData;
        }

        /// <summary>
        /// Gets all users associted with same Organization where the userName is working on
        /// </summary>
        /// <param name="userName">Main username</param>
        /// <returns>Enumerates AppUser entries to the organization</returns>
        public IEnumerable<User> GetAllUsersFromSameOrganization(string userName)
        {
            string sql = @"With OTEMP(OrganizationId)
                        AS
                        (
                             select Top 1 OU.OrgId from AppUser AU 
                             INNER JOIN OrgUser OU on OU.AppUserId = AU.AppUserId AND OU.IsDefaultOrg = 1
                             WHERE AU.AppUserEmail = @p0
                         )
                        SELECT AppUser.* from Org 
                        Inner Join OrgUser on Org.OrgId = OrgUser.OrgId 
                        Inner Join AppUser on AppUser.AppUserId = OrgUser.AppUserId
                        Inner Join OTEMP on OTEMP.OrganizationId = Org.OrgId";

            return db.Users.SqlQuery(sql, userName);
        }

        public IEnumerable<Role> GetUserRoles(Guid currentUserId)
        {
            string sql = @"select r.* from Role r
                            left join OrgRole orgR on r.RoleId = orgR.RoleId 
                            left join OrgUser ou on orgR.OrgId = ou.OrgId 
                            where ou.AppUserId = @p0";

            return db.Roles.SqlQuery(sql, currentUserId);
        }

        public bool DoesSuperAdminTokenExists(string userName, string superAdminTokenName)
        {
            bool hasData = false;
            try
            {
                string sql = @"select R.* from AppUser AU 
	                        INNER JOIN OrgUserRole OUL on OUL.AppUserId = AU.AppUserId 
	                        INNER JOIN OrgRole orl on orl.OrgRoleId = OUL.OrgRoleId 
	                        Inner Join Role r on orl.RoleId = r.RoleId and IsCustom = 1
	                        where R.RoleName = @p0
	                        and AU.AppUserEmail = @p1";
                hasData = db.Roles.SqlQuery(sql, superAdminTokenName, userName).Count() >= 1;

            }
            catch { }
            return hasData;
        }


        public IEnumerable<Feature> GetMenu(Guid? userId, Guid? roleId)
        { 
            return this.db.usp_GetMenu(roleId, userId);
        }


    }
}
