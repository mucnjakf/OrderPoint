using System.Diagnostics;
using OrderPoint.Admin.Health.Api;
using OrderPoint.Admin.Health.Dtos;
using OrderPoint.Admin.Health.Enumerations;
using OrderPoint.Shared.Errors;

namespace OrderPoint.Admin.Health.Services;

// One per circuit; an unreachable API is a result (unhealthy), not an error, so this skips ApiService
internal sealed partial class HealthService(HealthApiClient healthApiClient, ILogger<HealthService> logger)
{
    internal const string ApiCheckName = "API";

    internal event Action? Changed;

    internal IReadOnlyList<HealthCheckDto> Checks { get; private set; } = [];

    internal HealthStatus? Status { get; private set; }

    internal DateTimeOffset? CheckedAtUtc { get; private set; }

    internal bool IsChecking { get; private set; }

    internal async Task CheckAsync()
    {
        if (IsChecking)
        {
            return;
        }

        IsChecking = true;
        Changed?.Invoke();

        long startedAt = Stopwatch.GetTimestamp();

        try
        {
            HealthReportDto report = await healthApiClient.GetHealthAsync();

            SetReport(report, Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds);
        }
        catch (SessionExpiredException)
        {
            // Signed out; the layout shows the login form
        }
        catch (ApiException)
        {
            SetApiUnhealthy(Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds, "Responded with an error");
        }
        catch (Exception exception)
        {
            LogApiUnreachable(exception);
            SetApiUnhealthy(Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds, "Not reachable from the Admin");
        }
        finally
        {
            IsChecking = false;
            Changed?.Invoke();
        }
    }

    private void SetReport(HealthReportDto report, double roundTripMs)
    {
        // The API's own check only says it runs; the round trip also shows that the Admin reaches it
        Checks = report.Checks
            .Select(check => check.Name == ApiCheckName
                ? check with { DurationMs = roundTripMs, Description = "Reachable from the Admin" }
                : check)
            .ToList();

        Status = report.Status;
        CheckedAtUtc = DateTimeOffset.UtcNow;
    }

    // The exception details are in the logs; the card only says what happened
    private void SetApiUnhealthy(double roundTripMs, string description)
    {
        HealthCheckDto apiCheck = new(ApiCheckName, HealthStatus.Unhealthy, roundTripMs, description, Error: null);

        Checks = [apiCheck];
        Status = HealthStatus.Unhealthy;
        CheckedAtUtc = DateTimeOffset.UtcNow;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "The API health check could not reach the API")]
    private partial void LogApiUnreachable(Exception exception);
}