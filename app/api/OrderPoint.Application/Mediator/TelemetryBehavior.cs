using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderPoint.Application.Telemetry;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Mediator;

// A span, a duration metric and a log per command and query; never logs the request, which can hold passwords
internal sealed partial class TelemetryBehavior<TRequest, TResponse>(
    ApplicationTelemetry telemetry,
    ILogger<TelemetryBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private const string CommandRequestType = "Command";
    private const string QueryRequestType = "Query";

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;
        string requestType = request is IBaseCommand ? CommandRequestType : QueryRequestType;

        using Activity? activity = ApplicationTelemetry.ActivitySource.StartActivity(requestName);
        activity?.SetTag(ApplicationTelemetry.RequestNameTag, requestName);
        activity?.SetTag(ApplicationTelemetry.RequestTypeTag, requestType);

        long startedAt = Stopwatch.GetTimestamp();

        try
        {
            TResponse response = await next(cancellationToken);
            TimeSpan elapsed = Stopwatch.GetElapsedTime(startedAt);

            if (response.IsSuccess)
            {
                LogSucceeded(requestName, elapsed.TotalMilliseconds);
                telemetry.RecordRequest(requestName, requestType, null, elapsed);

                return response;
            }

            activity?.SetStatus(ActivityStatusCode.Error, response.Error.Description);
            activity?.SetTag(ApplicationTelemetry.ErrorTypeTag, response.Error.Code);

            LogFailed(requestName, response.Error.Code, response.Error.Description, elapsed.TotalMilliseconds);
            telemetry.RecordRequest(requestName, requestType, response.Error.Code, elapsed);

            return response;
        }
        catch (Exception exception)
        {
            // Logged by the API's exception handler; here only the span and the metric record it
            activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
            activity?.SetTag(ApplicationTelemetry.ErrorTypeTag, exception.GetType().FullName);
            activity?.AddException(exception);

            telemetry.RecordRequest(
                requestName,
                requestType,
                exception.GetType().FullName,
                Stopwatch.GetElapsedTime(startedAt));

            throw;
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "{RequestName} succeeded in {ElapsedMilliseconds:0} ms")]
    private partial void LogSucceeded(string requestName, double elapsedMilliseconds);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "{RequestName} failed with {ErrorCode} ({ErrorDescription}) in {ElapsedMilliseconds:0} ms")]
    private partial void LogFailed(
        string requestName,
        string errorCode,
        string errorDescription,
        double elapsedMilliseconds);
}