using Expense.Tracker.Data.EntityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense.Tracker.Data.DataUtilities.Base
{
    public abstract class BaseDataUtils
    {
        public DataManager Bridge { get; set; }

        public BaseDataUtils(DataManager bridge)
        {
            this.Bridge = bridge;
        }
         
        public ExpenseTrackerEntities DbFactory
        {
            get
            {
                return this.Bridge.DataFactory;
            }
        }
    }
}
