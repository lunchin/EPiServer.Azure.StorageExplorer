using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using EPiServer.Azure.Blobs;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace EPiServer.Azure.StorageExplorer
{
    public class StorageService : IStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly AzureBlobProviderOptions _azureBlobProviderOptions;

        public StorageService(IOptions<AzureBlobProviderOptions> options)
        {
            if (options.Value != null && !string.IsNullOrEmpty(options.Value.ConnectionString))
            {
                _blobServiceClient = new BlobServiceClient(options.Value.ConnectionString);
                _azureBlobProviderOptions = options.Value;
                IsInitialized = true;
            }
        }

        public string BlobRootUrl { get; }

        public bool IsInitialized { get; }

        public async Task<BlobClient> AddAsync(string containerName, string filename, Stream stream, long length)
        {
            var blob = await GetCloudBlockBlobAsync(containerName, filename);
            bool shouldUpload = true;
            if (blob.Exists())
            {
                var properties = blob.GetProperties();
                if (properties.Value?.ContentLength == length)
                {
                    shouldUpload = false;
                }
            }

            if (shouldUpload)
            {
                blob.Upload(stream);
            }

            return blob;
        }

        public async IAsyncEnumerable<BlobHierarchyItem> GetBlobItemsAsync(string containerName, string path)
        {
            var container = await GetContainerAsync(containerName);
            await foreach(var item in container.GetBlobsByHierarchyAsync(prefix:path, delimiter:"/"))
            {
                yield return item;
            }
        }

        public async Task<BlobClient> GetCloudBlockBlobAsync(string containerName, string path)
        {
            var container = await GetContainerAsync(containerName);
            return container.GetBlobClient(path);
        }

        public async Task<bool> ExistsAsync(string containerName, string path)
        {
            var reference = await GetCloudBlockBlobAsync(containerName, path);
            return reference.Exists();
        }

        public async Task DeleteAsync(string containerName, string path)
        {
            var reference = await GetCloudBlockBlobAsync(containerName, path);
            reference.DeleteIfExists();
        }

        public async Task DeleteAsync(string url)
        {
            var blobClient = new BlobClient(new Uri(url));
            await blobClient?.DeleteIfExistsAsync();
        }

        public async IAsyncEnumerable<BlobHierarchyItem> GetBlobDirectoriesAsync(string containerName, string path = null)
        {
            var container = await GetContainerAsync(containerName);
            await foreach(var dir in container.GetBlobsByHierarchyAsync(prefix:path, delimiter:"/"))
            {
                if (dir.IsPrefix)
                {
                    yield return dir;
                }
            }
        }

        public async Task<BlobHierarchyItem> GetBlobDirectoryAsync(string containerName, string path)
        {
            var container = await GetContainerAsync(containerName);
            BlobHierarchyItem returnItem;
            await foreach(var item in container.GetBlobsByHierarchyAsync(delimiter: "/", prefix: path))
            {
                if (item.IsPrefix)
                {
                    returnItem = item;
                }
                break;
            }
            
            return null;
        }

        public async Task<BlobContainerClient> GetContainerAsync(string containerName, 
            PublicAccessType blobContainerPublicAccessType = PublicAccessType.None)
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await blobContainerClient.CreateIfNotExistsAsync(blobContainerPublicAccessType);
            return blobContainerClient;
        }

        public async Task RenameAsync(BlobClient blob, string newName)
        {
            if (blob == null)
            {
                return;
            }

            var blobCopy = new BlobClient(_azureBlobProviderOptions.ConnectionString, blob.BlobContainerName, newName);
            if (blobCopy == null || await blobCopy.ExistsAsync() || !await blob.ExistsAsync())
            {
                return;
            }

            await blobCopy.StartCopyFromUriAsync(blob.Uri);
            await blob.DeleteIfExistsAsync();
        }

        public async IAsyncEnumerable<BlobContainerClient> GetContainersAsync()
        {
            await foreach (var container in _blobServiceClient.GetBlobContainersAsync())
            {
                yield return _blobServiceClient.GetBlobContainerClient(container.Name);
            }
        }

        public async Task<BlobClient> GetCloudBlockBlobAsync(string url)
        {
            var blobClient = new BlobClient(new Uri(url));
            var containerName = blobClient.BlobContainerName;
            var blobName = blobClient.Name;
            return await Task.FromResult(new BlobClient(_azureBlobProviderOptions.ConnectionString, containerName, blobName));
        }

    }
}
