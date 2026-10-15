using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OrderPoint.Shared.Images;

// Stored image URLs point at the storage emulator on this machine, which a phone can't reach, so the apps serve the
// images themselves under /images
internal static class ImageProxyExtensions
{
    internal const string StorageUrlKey = "Images:StorageUrl";

    internal const string ProxyPath = "/images/";

    private const string ParentDirectorySegment = "..";

    // A new upload always gets a new name, so an image never changes
    private const string CacheControl = "public, max-age=31536000, immutable";

    // On every HTTP client, so a new API client can't miss it
    internal static void AddImageProxy(this IServiceCollection services)
    {
        services.AddTransient<ImageUrlHandler>();
        services.ConfigureHttpClientDefaults(builder => builder.AddHttpMessageHandler<ImageUrlHandler>());
    }

    internal static void MapImageProxy(this WebApplication app)
    {
        if (string.IsNullOrWhiteSpace(app.Configuration[StorageUrlKey]))
        {
            return;
        }

        app.MapGet($"{ProxyPath}{{**path}}", GetImageAsync);
    }

    private static async Task<Results<FileStreamHttpResult, NotFound>> GetImageAsync(
        [FromRoute] string path,
        [FromServices] IConfiguration configuration,
        [FromServices] IHttpClientFactory httpClientFactory,
        HttpResponse response,
        CancellationToken cancellationToken)
    {
        // Only the images container, nothing above it
        if (path.Contains(ParentDirectorySegment, StringComparison.Ordinal))
        {
            return TypedResults.NotFound();
        }

        HttpClient httpClient = httpClientFactory.CreateClient();

        HttpResponseMessage storageResponse = await httpClient.GetAsync(
            $"{configuration[StorageUrlKey]}/{path}",
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (!storageResponse.IsSuccessStatusCode)
        {
            storageResponse.Dispose();

            return TypedResults.NotFound();
        }

        response.RegisterForDispose(storageResponse);
        response.Headers.CacheControl = CacheControl;

        Stream content = await storageResponse.Content.ReadAsStreamAsync(cancellationToken);

        return TypedResults.Stream(content, storageResponse.Content.Headers.ContentType?.ToString());
    }
}