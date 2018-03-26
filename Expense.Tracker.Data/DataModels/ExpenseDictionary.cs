using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense.Tracker.Data.DataModels
{
    public class ExpenseDictionary
    {
        public IEnumerable<ExpenseDataModel> ExpenseData { get; set; }

        public decimal ExpenseTotal { get; set; }
    }
}
