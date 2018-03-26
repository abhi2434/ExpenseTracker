using Expense.Tracker.Data.EntityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense.Tracker.Data.DataModels
{
    public class ExpenseDataModel
    {
        public ExpenseDataModel(Expens expenditure, IEnumerable<User> users)
        {
            this.ExpenseId = expenditure.ExpenseId;
            this.ExpenseHead = expenditure.ExpenseTitle;
            this.ExpenseDetail = expenditure.ExpenseDetail;
            if(expenditure.ExpenseTimeStamp.HasValue)
                this.ExpenseTimestamp = expenditure.ExpenseTimeStamp.Value.ToShortDateString();
            if (expenditure.UserId.HasValue)
            {
                var user = users.FirstOrDefault(e => e.UserId == expenditure.UserId);
                if (user != null)
                {
                    this.UserName = user.UserFullName;
                    this.LogoPath = user.ProfilePic;
                }
            }
            this.ExpenseAmount = expenditure.ExpenseAmount;
            this.Attachment = expenditure.AttachmentPath;
        }

        public string Attachment { get; private set; }
        public decimal? ExpenseAmount { get; private set; }
        public string ExpenseDetail { get; private set; }
        public string ExpenseHead { get; private set; }
        public Guid ExpenseId { get; private set; }
        public string ExpenseTimestamp { get; private set; }
        public string LogoPath { get; private set; }
        public string UserName { get; private set; }
    }
}
