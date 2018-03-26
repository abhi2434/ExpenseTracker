using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Utilities.Azure
{
    public class BlobStorageUtil : StorageBase
    {

        public BlobStorageUtil(string connectionstring)
            : base(connectionstring)
        {

        }

        private CloudBlobClient blobClient;
        private CloudBlobClient BlobClient
        {
            get
            {
                this.blobClient = this.blobClient ?? this.CurrentStorage.CreateCloudBlobClient();
                return this.blobClient;
            }
        }

        public CloudBlobContainer CreateContainer(string containerName, BlobContainerPublicAccessType permission = BlobContainerPublicAccessType.Blob)
        {
            var container = this.BlobClient.GetContainerReference(containerName);
            container.CreateIfNotExists();

            container.SetPermissions(new BlobContainerPermissions { PublicAccess = permission });
            return container;
        }

        public bool CopyBlob(CloudBlobContainer container, string sourceBlobName, string destinationBlobName)
        {

            try
            {
                CloudBlockBlob sourceblob = container.GetBlockBlobReference(sourceBlobName);
                CloudBlockBlob destinationblob = container.GetBlockBlobReference(destinationBlobName);
                destinationblob.StartCopyFromBlob(sourceblob);
                //sourceblob.DeleteIfExists();
                return true;
            }
            catch
            {
                return false;
            }

        }
        public CloudBlockBlob renameBlob(CloudBlobContainer container, string sourceBlobName, string destinationBlobName)
        {

            var NewdestinationBlobName = destinationBlobName;// +"_rename";  
            CloudBlockBlob sourceblob = container.GetBlockBlobReference(sourceBlobName);
            CloudBlockBlob destinationblob = container.GetBlockBlobReference(NewdestinationBlobName);
            destinationblob.StartCopyFromBlob(sourceblob);
            sourceblob.DeleteIfExists();
            return destinationblob;

        }

        public CloudBlockBlob CreateCloudBlob(CloudBlobContainer container, string filePath)
        {
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(filePath);
            using (var fileStream = File.OpenRead(filePath))
            {
                blockBlob.UploadFromStream(fileStream);
            }

            return blockBlob;
        }
        public CloudBlockBlob CreateCloudBlob(CloudBlobContainer container, Stream fileStream, string filePath)
        {
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(filePath);
            blockBlob.UploadFromStream(fileStream);

            return blockBlob;
        }
        public CloudBlockBlob CreateCloudBlob(CloudBlobContainer container, string fileContent, string filePath)
        {
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(filePath);
            blockBlob.UploadText(fileContent);

            return blockBlob;
        }
        public CloudBlockBlob CreateCloudBlob(CloudBlobContainer container, byte[] fileContent, string filePath)
        {
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(filePath);
            blockBlob.UploadFromByteArray(fileContent, 0, fileContent.Length);

            return blockBlob;
        }

        public CloudBlockBlob CopyCloudBlob(CloudBlobContainer container, CloudBlockBlob sourceblob, string targetFileName)
        {
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(targetFileName);
            blockBlob.StartCopyFromBlob(sourceblob);

            return blockBlob;
        }
        public IEnumerable<IListBlobItem> GetDirectoryList(string directoryName, string subDirectoryName)
        {
            var container = this.BlobClient.GetContainerReference(directoryName);

            return container.ListBlobs(subDirectoryName, false);
        }

        public void DeleteBlob(CloudBlobContainer container, string filePath)
        {
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(filePath);
            blockBlob.DeleteIfExists();
        }

        public Stream DownloadBlobAsStream(CloudBlobContainer container, string filePath)
        {
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(filePath);
            Stream theStream = new MemoryStream();
            blockBlob.DownloadToStream(theStream);

            return theStream;
        }
        public string DownloadBlobAsString(CloudBlobContainer container, string filePath)
        {
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(filePath);

            return blockBlob.DownloadText();
        }

        public string GetBlobUrlIfExists(CloudBlobContainer container, string filePath)
        {
            var blockBlob = container.GetBlockBlobReference(filePath);
            try
            {
                blockBlob.FetchAttributes();

                if (blockBlob.Exists() && blockBlob.Properties.Length > 0)
                    return blockBlob.Uri.AbsolutePath;
            }
            catch (Exception)
            {

            }
            return string.Empty;
        }


    }
}
