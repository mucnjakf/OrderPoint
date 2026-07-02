using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Exceptions;

internal sealed class RequestValidationExceptionHandler(IProblemDetailsService problemDetailsService)
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

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Type = nameof(ErrorType.RequestValidation),
                Detail = "Request validation error occured while processing your request",
                Extensions = new Dictionary<string, object?>
                {
                    { "errors", errors }
                }
            }
        });
    }
}