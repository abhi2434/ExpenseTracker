using System;
using System.Collections.Generic;
using System.Data.Entity.Core.EntityClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expense.Tracker.Data.EntityModel
{
    public class ConnectionBuilder
    {
        public ConnectionBuilder(string connectionString)
        {
            this.SetConnection(connectionString);
        }

        private void SetConnection(string providerString)
        {
            string providerName = "System.Data.SqlClient";
            EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder();
            entityBuilder.Provider = providerName;
            entityBuilder.ProviderConnectionString = providerString;

            entityBuilder.Metadata = @"res://*/EntityModel.ExpenseTracker.csdl|
                                    res://*/EntityModel.ExpenseTracker.ssdl|
                                    res://*/EntityModel.ExpenseTracker.msl";

            this.ConnectionString = entityBuilder.ToString();
        }

        public string ConnectionString { get; private set; }
    }
}
