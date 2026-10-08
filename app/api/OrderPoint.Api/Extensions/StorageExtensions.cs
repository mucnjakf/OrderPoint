using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace OrderPoint.Api.Extensions;

internal static class StorageExtensions
{
    internal static void CreateImageContainer(this WebApplication app)
    {
        var blobContainerClient = app.Services.GetRequiredService<BlobContainerClient>();
        blobContainerClient.CreateIfNotExists(PublicAccessType.Blob);
        blobContainerClient.SetAccessPolicy(PublicAccessType.Blob);
    }
}