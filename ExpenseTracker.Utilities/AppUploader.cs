using ExpenseTracker.Utilities.Azure;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Utilities
{
    public class AppUploader
    {
        public AppUploader(string connectionString)
        {
            this.Factory = new AzureStorageFactory(connectionString);
        }
        public AzureStorageFactory Factory { get; set; }

        public string UploadUserLogo(Stream FileStream, string fileName)
        {
            var container = this.Factory.BlobStore.CreateContainer("userlogos",
                BlobContainerPublicAccessType.Container);
            var blob = this.Factory.BlobStore.CreateCloudBlob(container, FileStream, fileName);
            return blob.Uri.AbsoluteUri;
        }

        public void RemoveFile(string filePath)
        {
            var container = this.Factory.BlobStore.CreateContainer("userlogos",
                BlobContainerPublicAccessType.Container);

            this.Factory.BlobStore.DeleteBlob(container, filePath);

        }
    }
}
