using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OrderPoint.Api.Configuration;
using StackExchange.Redis;

namespace OrderPoint.Api.Endpoints.Health;

/// <summary>The health of the API and its dependencies.</summary>
/// <param name="Data">The health report.</param>
internal sealed record GetHealthResponse(HealthReportResponse Data);

/// <summary>The result of every health check.</summary>
/// <param name="Status">The worst status of all checks.</param>
/// <param name="TotalDurationMs">How long all checks took together, in milliseconds.</param>
/// <param name="Checks">Each check: API, database, blob storage, cache and, in Development, the dev tools.</param>
internal sealed record HealthReportResponse(
    HealthStatus Status,
    double TotalDurationMs,
    IReadOnlyList<HealthCheckResponse> Checks);

/// <summary>The result of one health check.</summary>
/// <param name="Name">
/// What was checked: `API`, `Database`, `Storage`, `Cache`, `Database browser`, `Storage browser` or `Cache browser`.
/// </param>
/// <param name="Status">The status of this check.</param>
/// <param name="DurationMs">How long the check took, in milliseconds.</param>
/// <param name="Description">What the check reported, or `null`.</param>
/// <param name="Error">The error message when the check failed, or `null`.</param>
internal sealed record HealthCheckResponse(
    string Name,
    HealthStatus Status,
    double DurationMs,
    string? Description,
    string? Error);

// Health is a host concern, not a use case, so this uses the health check service directly instead of MediatR
internal sealed class GetHealthEndpoint : IEndpoint
{
    private const string TimedOutError = "No response in time; the service may be stopped or overloaded.";

    private const string CacheUnreachableError =
        "Can't connect to Redis; it may be stopped. Reads use memory and the database meanwhile.";

    // The names the checks are registered with (ServiceDefaults and the Aspire integrations)
    private static readonly Dictionary<string, string> DisplayNames = new()
    {
        ["self"] = "API",
        ["ApplicationDbContext"] = "Database",
        ["Azure_BlobContainerClient"] = "Storage",
        ["StackExchange.Redis"] = "Cache",
        ["pgweb"] = "Database browser",
        ["storage-blobs-explorer"] = "Storage browser",
        ["redisinsight"] = "Cache browser"
    };

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/health", HandleAsync)
            .WithName("GetHealth")
            .WithTags("Health")
            .WithSummary("Get health")
            .WithDescription("""
                Runs every health check and returns the result of each one: the API itself, the PostgreSQL
                database, the blob storage for images and the Redis cache. In Development it also checks the
                developer tools Aspire runs next to them (pgweb, Storage Explorer and RedisInsight).

                A stopped cache or developer tool reports `Degraded`, not `Unhealthy`, because the app keeps working
                without it: reads fall back to memory and the database.

                Always returns `200` with the report, also when a check fails; read `status` to know whether
                everything is healthy. Each check gives up after 5 seconds and then reports `Unhealthy`, so a
                stopped service never delays the report.

                The anonymous `/health` and `/alive` endpoints used by Aspire return only a status code and exist in
                Development only.
                """)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Ok<GetHealthResponse>> HandleAsync(
        [FromServices] HealthCheckService healthCheckService,
        CancellationToken cancellationToken)
    {
        HealthReport report = await healthCheckService.CheckHealthAsync(cancellationToken);

        List<HealthCheckResponse> checks = report.Entries
            .Select(entry => new HealthCheckResponse(
                DisplayNames.GetValueOrDefault(entry.Key, entry.Key),
                entry.Value.Status,
                entry.Value.Duration.TotalMilliseconds,
                entry.Value.Description,
                GetError(entry.Value.Exception)))
            .ToList();

        HealthReportResponse healthReport = new(report.Status, report.TotalDuration.TotalMilliseconds, checks);

        return TypedResults.Ok(new GetHealthResponse(healthReport));
    }

    // Timeouts ("The operation was canceled.") and Redis' long connection dump get a readable message
    private static string? GetError(Exception? exception)
    {
        return exception switch
        {
            null => null,
            OperationCanceledException => TimedOutError,
            RedisConnectionException => CacheUnreachableError,
            _ => exception.Message
        };
    }
}