using Expense.Tracker.Data;
using Expense.Tracker.Data.DataModels;
using Expense.Tracker.Data.DataUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Expense.Tracker.Security
{
    public class SecurityHandler
    {

        #region Data Members

        UserDataUtil userDataManager = null;

        public string UserId { get; set; }

        public string CurrentController { get; set; }
        public string CurrentAction { get; set; }
        public HttpContextBase CurrentContext { get; set; }

        # endregion

        # region CTors
        public SecurityHandler() { }
        public SecurityHandler(UserDataUtil userUtil)
        {
            this.userDataManager = userUtil;
        }
        public SecurityHandler(string userId, HttpContextBase context, DataManager manager)
        {
            userDataManager = manager.User;

            this.UserId = userId;

            this.CurrentContext = context;
        }
        public SecurityHandler(string userId, HttpContextBase context, UserDataUtil userDataUtil)
        {
            userDataManager = userDataUtil;

            this.UserId = userId;

            this.CurrentContext = context;
        }

        public SecurityHandler(HttpContext httpContext)
        {
            // TODO: Complete member initialization
            this.CurrentContext = new HttpContextWrapper(httpContext);
        }
        # endregion

        # region Member Functions

        public bool HasRequiredToken(string controller, string action)
        {
            bool hasToken = false;
            this.CurrentController = controller;
            this.CurrentAction = action;
            if (this.CurrentAction.Equals("Index")) // We need this because Index is default action
                this.CurrentAction = string.Empty;
            //Determine the Feature request
            var currentFeature = this.GetSpecificFeature();
            if (currentFeature != null)
            {
                hasToken = this.HasRoleFeature(this.UserId, currentFeature);
            }
            return hasToken; //We allow the feature that is not present in database
        }

        public bool HasSuperAdminToken(string userName)
        {
            string superAdminTokenName = Constants.CON_SUPERADMIN_TOKEN;
            return this.userDataManager.DoesSuperAdminTokenExists(userName, superAdminTokenName);
        }

        private bool HasRoleFeature(string userId, FeatureModel currentFeature)
        {
            var roleId = HttpContext.Current.Session[Constants.CON_ROLE];
            if (roleId == null)
                return this.userDataManager.DoesFeatureExists(userId, currentFeature.FeatureId);

            var roleGuid = Guid.Parse(roleId.ToString());

            return this.HasRoleFeature(roleGuid, currentFeature.FeatureId);

        }

        private bool HasRoleFeature(Guid roleGuid, Guid featureId)
        {
            var roleFeature = HttpContext.Current.Application[roleGuid.ToString()] as RoleFeatureModel;

            if (roleFeature == null)
            {
                roleFeature = this.userDataManager.GetRoleFeature(roleGuid);
                HttpContext.Current.Application[roleGuid.ToString()] = roleFeature;
            }

            return roleFeature.HasFeature(featureId);

        }

        private FeatureModel GetSpecificFeature()
        {
            var features = this.GetApplicationFeatures();

            return features.FirstOrDefault(f => !string.IsNullOrWhiteSpace(f.Controller)
                                            && f.Controller.Equals(this.CurrentController, StringComparison.InvariantCultureIgnoreCase)
                                            && f.Action.Equals(this.CurrentAction, StringComparison.InvariantCultureIgnoreCase));
        }

        private List<FeatureModel> GetApplicationFeatures()
        {
            List<FeatureModel> features = null;
            if (this.CurrentContext.Application[Constants.CON_APP_FEATURES] != null)
            {
                features = this.CurrentContext.Application[Constants.CON_APP_FEATURES] as List<FeatureModel>;
            }
            else
            {
                var thefeatures = this.userDataManager.GetAllFeatures();
                List<FeatureModel> simpleFeatures = new List<FeatureModel>();
                foreach (var feature in thefeatures)
                {
                    simpleFeatures.Add(new FeatureModel(feature));
                }
                this.CurrentContext.Application[Constants.CON_APP_FEATURES] = simpleFeatures;

                features = simpleFeatures;
            }

            return features;
        }

        public void ClearApplicationFeatures()
        {
            if (this.CurrentContext.Application[Constants.CON_APP_FEATURES] != null)
                this.CurrentContext.Application.Remove(Constants.CON_APP_FEATURES);
        }
        # endregion
    }
}
