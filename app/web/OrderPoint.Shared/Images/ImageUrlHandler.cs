using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;

namespace OrderPoint.Shared.Images;

// Points the image URLs in API responses at this app's /images, which serves them from storage
internal sealed class ImageUrlHandler(IConfiguration configuration) : DelegatingHandler
{
    private const string JsonMediaTypeSuffix = "json";

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

        string? storageUrl = configuration[ImageProxyExtensions.StorageUrlKey];
        MediaTypeHeaderValue? contentType = response.Content.Headers.ContentType;

        if (string.IsNullOrWhiteSpace(storageUrl)
            || contentType?.MediaType?.EndsWith(JsonMediaTypeSuffix, StringComparison.Ordinal) != true)
        {
            return response;
        }

        string content = await response.Content.ReadAsStringAsync(cancellationToken);
        response.Content.Dispose();

        response.Content = new StringContent(
            content.Replace($"{storageUrl}/", ImageProxyExtensions.ProxyPath, StringComparison.Ordinal),
            contentType);

        return response;
    }
}