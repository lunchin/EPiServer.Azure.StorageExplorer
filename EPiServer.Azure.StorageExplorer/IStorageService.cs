using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;

namespace EPiServer.Azure.StorageExplorer
{
    public interface IStorageService
    {
        string BlobRootUrl { get; }
        bool IsInitialized { get; }
        List<IListBlobItem> GetBlobItems(string containerName, string path, int index, int length);
        List<CloudBlobDirectory> GetBlobDirectories(string containerName, string path = null);
        CloudBlobDirectory GetBlobDirectory(string containerName, string path);
        CloudBlobContainer GetContainer(string containerName, 
            BlobContainerPublicAccessType blobContainerPublicAccessType = BlobContainerPublicAccessType.Off);
        List<CloudBlobContainer> GetContainers();
        CloudBlockBlob GetCloudBlockBlob(string containerName, string path);
        CloudBlockBlob GetCloudBlockBlob(string url);
        CloudBlockBlob Add(string containerName, string filename, Stream stream, long length);
        void Delete(string containerName, string path);
        void Delete(string url);
        void Rename(CloudBlockBlob blob, string newName);
        bool Exists(string containerName, string path);
    }
}
