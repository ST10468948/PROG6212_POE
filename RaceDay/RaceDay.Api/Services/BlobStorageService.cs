using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace RaceDay.Api.Services
{
    public class BlobStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public BlobStorageService(IConfiguration configuration)
        {
            string connectionString = configuration["AzureBlobStorage:ConnectionString"] ?? "UseDevelopmentStorage=true";
            string containerName = configuration["AzureBlobStorage:ContainerName"] ?? "raceday-media";
            _containerClient = new BlobContainerClient(connectionString, containerName);
        }

        public async Task<string> UploadAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file provided.");

            await _containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            string extension = Path.GetExtension(file.FileName);
            string blobName = $"{folder}/{Guid.NewGuid()}{extension}";
            BlobClient blobClient = _containerClient.GetBlobClient(blobName);

            await using Stream stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });

            return blobClient.Uri.ToString();
        }
    }
}