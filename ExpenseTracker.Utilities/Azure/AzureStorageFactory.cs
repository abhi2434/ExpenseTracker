using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Utilities.Azure
{
    public class AzureStorageFactory
    {
        public AzureStorageFactory(string connectionString)
        {
            this.ConnectionString = connectionString;
        }
        private BlobStorageUtil _blobStorage;
       
        public string ConnectionString { get; set; }
        public BlobStorageUtil BlobStore
        {
            get
            {
                this._blobStorage = this._blobStorage ?? new BlobStorageUtil(this.ConnectionString);
                return this._blobStorage;
            }
        }

    }
}
