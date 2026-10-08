using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using OrderPoint.Application.Storage;

namespace OrderPoint.Infrastructure.Storage;

internal sealed class BlobImageStorage(BlobContainerClient blobContainerClient) : IImageStorage
{
    public async Task<string> UploadAsync(
        Stream content,
        string contentType,
        string folder,
        CancellationToken cancellationToken = default)
    {
        BlobClient blobClient = blobContainerClient.GetBlobClient($"{folder}/{Guid.CreateVersion7()}");

        var options = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        };

        await blobClient.UploadAsync(content, options, cancellationToken);

        return blobClient.Uri.ToString();
    }

    public async Task DeleteAsync(string imageUrl, CancellationToken cancellationToken = default)
    {
        var blobUriBuilder = new BlobUriBuilder(new Uri(imageUrl));

        await blobContainerClient.DeleteBlobIfExistsAsync(
            blobUriBuilder.BlobName,
            cancellationToken: cancellationToken);
    }
}