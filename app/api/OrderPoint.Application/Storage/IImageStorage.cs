namespace OrderPoint.Application.Storage;

public interface IImageStorage
{
    Task<string> UploadAsync(
        Stream content,
        string contentType,
        string folder,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(string imageUrl, CancellationToken cancellationToken = default);
}