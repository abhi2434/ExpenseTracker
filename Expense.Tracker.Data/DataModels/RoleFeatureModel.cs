using Expense.Tracker.Data.EntityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense.Tracker.Data.DataModels
{
    public class RoleFeatureModel
    {
        private Role role;
         

        public RoleFeatureModel(Role role)
        {
            this.RoleId = role.RoleId;
            this.FeatureIds = new List<Guid>();
            foreach (var feature in role.RoleFeatures)
            {
                this.FeatureIds.Add(feature.FeatureId);
            }
        }
        public Guid RoleId { get; set; }
        public List<Guid> FeatureIds { get; set; }

        public bool HasFeature(Guid featureId)
        {
            if (this.FeatureIds != null)
                return this.FeatureIds.Contains(featureId);

            return false;
        }
    }
}
