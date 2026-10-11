using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace OrderPoint.ServiceDefaults;

// Shared by every service: OpenTelemetry (logs, traces, metrics), health checks, service discovery and resilience
public static class ServiceDefaultsExtensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";
    private const string LiveHealthCheckTag = "live";
    private const string SelfHealthCheckName = "self";
    private const string OtlpEndpointConfigurationKey = "OTEL_EXPORTER_OTLP_ENDPOINT";

    // Well under the Admin's 10 s attempt timeout, so the report always arrives
    private static readonly TimeSpan HealthCheckTimeout = TimeSpan.FromSeconds(5);

    // Our own sources and meters (e.g. OrderPoint.Application)
    private const string OrderPointTelemetryName = "OrderPoint.*";

    // Blazor Server: navigation, event handling and circuit spans
    private const string BlazorActivitySourceName = "Microsoft.AspNetCore.Components";

    // Blazor Server: component rendering, lifecycle and circuit metrics
    private static readonly string[] BlazorMeterNames =
    [
        "Microsoft.AspNetCore.Components",
        "Microsoft.AspNetCore.Components.Lifecycle",
        "Microsoft.AspNetCore.Components.Server.Circuits"
    ];

    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();
        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Retrying a POST, PUT or DELETE that timed out could create or delete something twice
            http.AddStandardResilienceHandler(options => options.Retry.DisableForUnsafeHttpMethods());
            http.AddServiceDiscovery();
        });

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return app;
        }

        // Ready: every check passes (used by Aspire to start dependent resources)
        app.MapHealthChecks(HealthEndpointPath);

        // Alive: only the checks tagged "live", so a failing dependency does not restart the service
        app.MapHealthChecks(AlivenessEndpointPath, new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains(LiveHealthCheckTag)
        });

        return app;
    }

    private static void ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        OpenTelemetryBuilder openTelemetry = builder.Services
            .AddOpenTelemetry()
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddMeter(BlazorMeterNames)
                .AddMeter(OrderPointTelemetryName))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation(options => options.Filter = IsNotHealthCheckRequest)
                .AddHttpClientInstrumentation()
                .AddSource(BlazorActivitySourceName)
                .AddSource(OrderPointTelemetryName));

        // Set by Aspire; without it (e.g. dotnet run on its own) telemetry is collected but not exported
        if (!string.IsNullOrWhiteSpace(builder.Configuration[OtlpEndpointConfigurationKey]))
        {
            openTelemetry.UseOtlpExporter();
        }
    }

    private static void AddDefaultHealthChecks(this IHostApplicationBuilder builder)
    {
        builder.Services
            .AddHealthChecks()
            .AddCheck(SelfHealthCheckName, () => HealthCheckResult.Healthy(), [LiveHealthCheckTag]);

        // A stopped dependency can hang instead of failing; post-configure also covers the integrations' checks
        builder.Services.PostConfigure<HealthCheckServiceOptions>(options =>
        {
            foreach (HealthCheckRegistration registration in options.Registrations
                         .Where(registration => registration.Timeout == Timeout.InfiniteTimeSpan))
            {
                registration.Timeout = HealthCheckTimeout;
            }
        });
    }

    private static bool IsNotHealthCheckRequest(HttpContext context)
    {
        return !context.Request.Path.StartsWithSegments(HealthEndpointPath)
            && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath);
    }
}