using Expense.Tracker.Data.EntityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense.Tracker.Data.DataModels
{
    public class FeatureModel
    {
        public FeatureModel(Feature theFeature)
        {
            this.FeatureId = theFeature.FeatureId;
            this.Controller = theFeature.Controller;
            this.Action = theFeature.Action;
        }
        public Guid FeatureId { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
    }
}
