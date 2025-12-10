using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Skribe.Shared.Data;

public class BlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    public BlobStorageService(IConfiguration configuration)
    {
        _blobServiceClient = new BlobServiceClient(configuration.GetConnectionString("BlobStorage"));
    }

    public async Task UploadJournalEntryAsync(string userId, object data)
    {
        var containerName = "journal-entries";
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();

        var blobName = $"{userId}/{DateTime.UtcNow:yyyy--MM-dd}.json";
        var blobClient = containerClient.GetBlobClient(blobName);

        var jsonContent = JsonSerializer.Serialize(data);
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonContent));

        await blobClient.UploadAsync(stream, overwrite: true);
    }

}


