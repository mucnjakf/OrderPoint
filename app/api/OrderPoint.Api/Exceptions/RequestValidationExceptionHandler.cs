using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Exceptions;

internal sealed partial class RequestValidationExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<RequestValidationExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        Error[] errors = validationException.Errors
            .Select(failure => Error.RequestValidation(
                $"RequestValidation.{failure.PropertyName}",
                failure.ErrorMessage))
            .ToArray();

        LogValidationFailed(
            httpContext.Request.Method,
            httpContext.Request.Path,
            string.Join(", ", errors.Select(error => error.Code).Distinct()));

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Type = nameof(ErrorType.RequestValidation),
                Detail = "Request validation error occurred while processing your request",
                Extensions = new Dictionary<string, object?>
                {
                    { "errors", errors }
                }
            }
        });
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Request validation failed for {Method} {Path}: {ErrorCodes}")]
    private partial void LogValidationFailed(string method, string path, string errorCodes);
}