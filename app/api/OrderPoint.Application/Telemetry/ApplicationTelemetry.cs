using System.Diagnostics;
using System.Diagnostics.Metrics;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Telemetry;

// Spans and metrics of OrderPoint's own use cases; ServiceDefaults exports every "OrderPoint.*" source and meter
internal sealed class ApplicationTelemetry
{
    internal const string Name = "OrderPoint.Application";

    internal static readonly ActivitySource ActivitySource = new(Name);

    // OpenTelemetry's standard attribute for the kind of error; here the error code (e.g. "Item.NotFound")
    internal const string ErrorTypeTag = "error.type";
    internal const string RequestNameTag = "orderpoint.request.name";
    internal const string RequestTypeTag = "orderpoint.request.type";
    private const string UserRoleTag = "orderpoint.user.role";

    private readonly Histogram<double> _requestDuration;
    private readonly Counter<long> _ordersPlaced;
    private readonly Histogram<double> _orderValue;
    private readonly Counter<long> _logins;

    public ApplicationTelemetry(IMeterFactory meterFactory)
    {
        Meter meter = meterFactory.Create(Name);

        _requestDuration = meter.CreateHistogram<double>(
            "orderpoint.request.duration",
            unit: "s",
            description: "Duration of commands and queries");

        _ordersPlaced = meter.CreateCounter<long>(
            "orderpoint.orders.placed",
            unit: "{order}",
            description: "Orders placed by customers");

        _orderValue = meter.CreateHistogram<double>(
            "orderpoint.orders.value",
            unit: "EUR",
            description: "Total of each placed order");

        _logins = meter.CreateCounter<long>(
            "orderpoint.auth.logins",
            unit: "{login}",
            description: "Login attempts by role and outcome");
    }

    internal void RecordRequest(string requestName, string requestType, string? errorType, TimeSpan duration)
    {
        TagList tags = new()
        {
            { RequestNameTag, requestName },
            { RequestTypeTag, requestType }
        };

        if (errorType is not null)
        {
            tags.Add(ErrorTypeTag, errorType);
        }

        _requestDuration.Record(duration.TotalSeconds, tags);
    }

    internal void RecordOrderPlaced(decimal total)
    {
        _ordersPlaced.Add(1);
        _orderValue.Record((double)total);
    }

    internal void RecordLogin(UserRole role, Result result)
    {
        TagList tags = new() { { UserRoleTag, role.ToString() } };

        if (result.IsFailure)
        {
            tags.Add(ErrorTypeTag, result.Error.Code);
        }

        _logins.Add(1, tags);
    }
}