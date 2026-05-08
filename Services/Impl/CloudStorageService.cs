using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using ApiBackend.Services.Interfaces;

namespace ApiBackend.Services.Impl
{
    public class CloudStorageService : ICloudStorageService
    {
        private readonly string _connectionString;
        private readonly string _precontainerName;
        private readonly string _postcontainerName;

        public CloudStorageService(IConfiguration configuration)
        {
            _connectionString = configuration["AzureBlobStorag:ConnectionString"]
                ?? throw new InvalidOperationException("AzureBlobStorage:ConnectionString is missing.");
            _precontainerName = configuration["AzureBlobStorag:PreContainerName"]
                ?? throw new InvalidOperationException("AzureBlobStorage:ContainerName is missing.");
            _postcontainerName = configuration["AzureBlobStorag:PostContainerName"]
                ?? throw new InvalidOperationException("AzureBlobStorage:PostContainerName is missing.");
        }

        public async Task<string> UploadImageAsync(IFormFile image)
        {
            var containerClient = new BlobContainerClient(_connectionString, _precontainerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None); // Private container

            var extension = Path.GetExtension(image.FileName);
            var blobName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}{extension}";
            var blobClient = containerClient.GetBlobClient(blobName);

            using var stream = image.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobHttpHeaders
            {
                ContentType = image.ContentType
            });

            // Ham blob URL döner (SAS token olmadan) — DB'de bu saklanır
            return blobClient.Uri.ToString();
        }

        public string PreImageGenerateSasUrl(string blobUrl, TimeSpan expiry)
        {
            // Ham URL'den blob adını parse et
            var uri = new Uri(blobUrl);
            var blobName = uri.AbsolutePath.TrimStart('/').Replace($"{_precontainerName}/", "");

            var containerClient = new BlobContainerClient(_connectionString, _precontainerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            // SAS token builder — Read izni, 24 saat geçerli
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _precontainerName,
                BlobName = blobName,
                Resource = "b", // b = blob
                ExpiresOn = DateTimeOffset.UtcNow.Add(expiry),
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            return blobClient.GenerateSasUri(sasBuilder).ToString();
        }

        public string PostImageGenerateSasUrl(string blobUrl, TimeSpan expiry)
        {
            // Ham URL'den blob adını parse et
            var uri = new Uri(blobUrl);
            var blobName = uri.AbsolutePath.TrimStart('/').Replace($"{_postcontainerName}/", "");

            var containerClient = new BlobContainerClient(_connectionString, _postcontainerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            // SAS token builder — Read izni, 24 saat geçerli
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _postcontainerName,
                BlobName = blobName,
                Resource = "b", // b = blob
                ExpiresOn = DateTimeOffset.UtcNow.Add(expiry),
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            return blobClient.GenerateSasUri(sasBuilder).ToString();
        }

        public string GenerateSasUrl(string blobUrl, TimeSpan expiry)
        {
            // Ham URL'den blob adını parse et
            var uri = new Uri(blobUrl);
            var blobName = uri.AbsolutePath.TrimStart('/').Replace($"{_postcontainerName}/", "");

            var containerClient = new BlobContainerClient(_connectionString, _postcontainerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            // SAS token builder — Read izni, 24 saat geçerli
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _postcontainerName,
                BlobName = blobName,
                Resource = "b", // b = blob
                ExpiresOn = DateTimeOffset.UtcNow.Add(expiry),
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            return blobClient.GenerateSasUri(sasBuilder).ToString();
        }
        
        
    }
}