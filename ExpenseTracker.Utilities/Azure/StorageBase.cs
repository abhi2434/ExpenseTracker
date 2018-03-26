using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Utilities.Azure
{
    public class StorageBase
    {
        CloudStorageAccount storageAccount;
        public CloudStorageAccount CurrentStorage
        {
            get
            {
                this.storageAccount = this.storageAccount ?? CloudStorageAccount.Parse(this.ConnectionString);
                return this.storageAccount;
            }
        }

        public string ConnectionString { get; set; }

        public StorageBase(string connectionString)
        {
            this.ConnectionString = connectionString;
        }
    }
}

