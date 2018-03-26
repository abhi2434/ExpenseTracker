using ExpenseTracker.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense.Tracker.Data.EntityModel
{
    public partial class ExpenseTrackerEntities : DbContext
    {
        public ExpenseTrackerEntities(string connectionString)
            : base(connectionString) { }

        public override int SaveChanges()
        {
            var theUserName = Web.SessionUser;
            if (string.IsNullOrEmpty(theUserName))
            {
                theUserName = Web.UserName;
                if (string.IsNullOrEmpty(theUserName))
                    theUserName = "Not found";
            }

            var changeSet = ChangeTracker.Entries();
            if (changeSet != null)
            {
                foreach (var entity in changeSet)
                {
                    if (entity.State == EntityState.Added)
                    {
                        if (entity.Entity.GetType().GetProperty("CreatedOn") != null)
                        {
                            entity.Entity.GetType().GetProperty("CreatedOn").SetValue(entity.Entity, DateTime.Now);
                        }
                        if (entity.Entity.GetType().GetProperty("LastUpdatedOn") != null)
                        {
                            entity.Entity.GetType().GetProperty("LastUpdatedOn").SetValue(entity.Entity, DateTime.Now);
                        }
                        if (entity.Entity.GetType().GetProperty("AutoAudit_CreatedBy") != null)
                        {
                            entity.Entity.GetType().GetProperty("AutoAudit_CreatedBy").SetValue(entity.Entity, theUserName);
                        }
                        if (entity.Entity.GetType().GetProperty("AutoAudit_CreatedDate") != null)
                        {
                            entity.Entity.GetType().GetProperty("AutoAudit_CreatedDate").SetValue(entity.Entity, DateTime.UtcNow);
                        }

                    }
                    else if (entity.State == EntityState.Modified)
                    {
                        if (entity.Entity.GetType().GetProperty("LastUpdatedOn") != null)
                        {
                            entity.Entity.GetType().GetProperty("LastUpdatedOn").SetValue(entity.Entity, DateTime.Now);
                        }
                        if (entity.Entity.GetType().GetProperty("AutoAudit_ModifiedBy") != null)
                        {
                            entity.Entity.GetType().GetProperty("AutoAudit_ModifiedBy").SetValue(entity.Entity, theUserName);
                        }
                        if (entity.Entity.GetType().GetProperty("AutoAudit_ModifiedDate") != null)
                        {
                            entity.Entity.GetType().GetProperty("AutoAudit_ModifiedDate").SetValue(entity.Entity, DateTime.UtcNow);
                        }
                    }





                }
            }

            if (this.Database.Connection.State == ConnectionState.Closed)
                this.Database.Connection.Open();

            return base.SaveChanges();

        }
    }
}
