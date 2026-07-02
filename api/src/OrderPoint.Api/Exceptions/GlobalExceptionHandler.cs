using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace OrderPoint.Api.Exceptions;

internal sealed class GlobalExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken) => problemDetailsService.TryWriteAsync(new ProblemDetailsContext
    {
        HttpContext = httpContext,
        Exception = exception,
        ProblemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error",
            Type = exception.GetType().Name,
            Detail = "Unknown error occured while processing your request",
        }
    });
}