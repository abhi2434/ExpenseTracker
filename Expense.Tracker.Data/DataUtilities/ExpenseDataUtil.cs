using Expense.Tracker.Data.DataModels;
using Expense.Tracker.Data.DataUtilities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense.Tracker.Data.DataUtilities
{
    public class ExpenseDataUtil : BaseDataUtils
    {
        public ExpenseDataUtil(DataManager bridge)
            : base(bridge)
        {

        }

        public ExpenseDictionary GetExpenseData(int? days, string search)
        {
            var expenses = this.GetExpenses(days, search);
            decimal expenseTotal = 0;
            foreach (var expense in expenses)
                expenseTotal += expense.ExpenseAmount.HasValue ? expense.ExpenseAmount.Value : 0;
            var dict = new ExpenseDictionary { ExpenseData = expenses, ExpenseTotal = expenseTotal };

            return dict;
        }

        
        public IEnumerable<ExpenseDataModel> GetExpenses(int? days, string search)
        {
            var expenses = this.DbFactory.Expenses.AsEnumerable();
            if (days.HasValue)
            {
                var startDay = days.Value * (-1);
                var startDate = DateTime.Now.AddDays(startDay);
                var endDate = DateTime.Now;
                expenses = expenses.Where(e => e.ExpenseTimeStamp >= startDate && e.ExpenseTimeStamp <= endDate);
            }
            if (!string.IsNullOrEmpty(search))
                expenses = expenses.Where(e => e.ExpenseTitle.Contains(search));

            var users = this.DbFactory.Users.ToList();
            foreach(var expense in expenses)
            {
                
                yield return new ExpenseDataModel(expense, users);
            }
        }

    }
}
