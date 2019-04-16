using EPiServer.ServiceLocation;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace EPiServer.Azure.StorageExplorer
{
    [ServiceConfiguration(ServiceType = typeof(IStorageService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class StorageService : IStorageService
    {
        private readonly StorageCredentials _storageCredentials;

        public StorageService()
        {
            if (CloudStorageAccount.TryParse(ConfigurationManager.ConnectionStrings["EPiServerAzureBlobs"].ConnectionString, out var account))
            {
                _storageCredentials = account.Credentials;
                BlobRootUrl = VirtualPathUtility.AppendTrailingSlash(account.BlobStorageUri.PrimaryUri.ToString());
                IsInitialized = true;
            }
        }

        public string BlobRootUrl { get; }

        public bool IsInitialized { get; }

        public CloudBlockBlob Add(string containerName, string filename, Stream stream, long length)
        {
            var blob = GetCloudBlockBlob(containerName, filename);
            bool shouldUpload = true;
            if (blob.Exists())
            {
                blob.FetchAttributes();
                if (blob.Properties.Length == length)
                {
                    shouldUpload = false;
                }
            }

            if (shouldUpload)
            {
                blob.UploadFromStream(stream);
            }

            return blob;
        }

        public List<IListBlobItem> GetBlobItems(string containerName, string path, int index, int length)
        {
            var container = GetContainer(containerName);
            var blobRequestOptions = new BlobRequestOptions();
            var operationContext = new OperationContext();

            return container.ListBlobs(path, false, BlobListingDetails.Metadata, blobRequestOptions, operationContext)
                .Skip(index)
                .Take(length)
                .ToList();
        }

        public CloudBlockBlob GetCloudBlockBlob(string containerName, string path)
        {
            var container = GetContainer(containerName);
            return container.GetBlockBlobReference(path);
        }

        public bool Exists(string containerName, string path)
        {
            var reference = GetCloudBlockBlob(containerName, path);
            return reference.Exists();
        }

        public void Delete(string containerName, string path)
        {
            var reference = GetCloudBlockBlob(containerName, path);
            reference.DeleteIfExists();
        }

        public void Delete(string url)
        {
            var storageAccount = new CloudStorageAccount(_storageCredentials, true);
            var blobClient = storageAccount.CreateCloudBlobClient();
            blobClient.GetBlobReferenceFromServer(new System.Uri(url))?.DeleteIfExists();
        }

        public List<CloudBlobDirectory> GetBlobDirectories(string containerName, string path = null)
        {
            var container = GetContainer(containerName);
            var directory = container.GetDirectoryReference(path);
            if (directory == null)
            {
                return new List<CloudBlobDirectory>();
            }
            return GetFolders(containerName, directory);
        }

        public CloudBlobDirectory GetBlobDirectory(string containerName, string path)
        {
            var container = GetContainer(containerName);
            return container.GetDirectoryReference(path);
        }

        public CloudBlobContainer GetContainer(string containerName, 
            BlobContainerPublicAccessType blobContainerPublicAccessType = BlobContainerPublicAccessType.Off)
        {
            var storageAccount = new CloudStorageAccount(_storageCredentials, true);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(containerName);
            blobContainer.CreateIfNotExists(blobContainerPublicAccessType);
            return blobContainer;
        }

        public void Rename(CloudBlockBlob blob, string newName)
        {
            if (blob == null)
            {
                return;
            }

            var blobCopy = blob.Container.GetBlockBlobReference(newName);

            if (blobCopy == null || blobCopy.Exists() || !blob.Exists())
            {
                return;
            }
            blobCopy.StartCopy(blob);
            blob.DeleteIfExists();
        }

        public List<CloudBlobContainer> GetContainers()
        {
            var storageAccount = new CloudStorageAccount(_storageCredentials, true);
            var blobClient = storageAccount.CreateCloudBlobClient();
            return blobClient.ListContainers().ToList();
        }


        public CloudBlockBlob GetCloudBlockBlob(string url)
        {
            var storageAccount = new CloudStorageAccount(_storageCredentials, true);
            var blobClient = storageAccount.CreateCloudBlobClient();
            return blobClient.GetBlobReferenceFromServer(new System.Uri(url)) as CloudBlockBlob;
        }

        private List<CloudBlobDirectory> GetFolders(string containerName, CloudBlobDirectory blobDirectory = null)
        {
            if (blobDirectory == null)
            {
                var container = GetContainer(containerName);
                var items = container.GetItems(string.Empty).Result;
                return items.OfType<CloudBlobDirectory>().ToList();
            }
            var children = blobDirectory.GetItems().Result;
            return children.OfType<CloudBlobDirectory>().ToList();
        }
    }
}
