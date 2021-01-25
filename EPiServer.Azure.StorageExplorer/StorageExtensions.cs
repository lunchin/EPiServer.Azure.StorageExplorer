using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;

namespace EPiServer.Azure.StorageExplorer
{
    public static class StorageExtensions
    {
        public static AzureBlob GetAzureBlob(this BlobContainerClient item)
        {
            var properties = item.GetProperties().Value;
            return new AzureBlob
            {
                Name = item.Name,
                IsContainer = true,
                LastModified = properties?.LastModified.UtcDateTime ?? DateTime.UtcNow,
                Etag = properties?.ETag.ToString()
            };
        }

        public static AzureBlob GetAzureBlob(this BlobHierarchyItem item, BlobContainerClient container)
        {
            
            if (item.IsPrefix)
            {
                return new AzureBlob
                {
                    Name = item.Prefix,
                    IsDirectory = true,
                    Url =  container.Uri + "/" + item.Prefix                };
            }

            if (item.Blob != null)
            {
                return new AzureBlob
                {
                    Name = item.Blob.Name.Substring(item.Blob.Name.LastIndexOf('/') == 0 ? 0 : item.Blob.Name.LastIndexOf('/') + 1),
                    IsBlob = true,
                    LastModified = item.Blob.Properties?.LastModified?.UtcDateTime ?? DateTime.UtcNow,
                    Etag = item.Blob.Properties?.ETag?.ToString(),
                    BlobType = item.Blob.Properties?.BlobType.ToString(),
                    ContentType = item.Blob.Properties?.ContentType,
                    Size = item.Blob.Properties?.ContentLength ?? 0,
                    Status = item.Blob.Properties?.LeaseStatus.ToString(),
                    Url = container.GetBlobClient(item.Blob.Name).Uri.ToString()
                };
            }
            return null;
        }
    }
}
