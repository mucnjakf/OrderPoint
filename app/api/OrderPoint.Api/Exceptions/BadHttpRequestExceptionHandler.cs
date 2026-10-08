using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Exceptions;

internal sealed class BadHttpRequestExceptionHandler(IProblemDetailsService problemDetailsService)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not BadHttpRequestException badHttpRequestException)
        {
            return false;
        }

        httpContext.Response.StatusCode = badHttpRequestException.StatusCode;

        Error[] errors =
        [
            Error.RequestValidation("RequestValidation.BadHttpRequest", badHttpRequestException.Message)
        ];

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = badHttpRequestException.StatusCode,
                Title = ReasonPhrases.GetReasonPhrase(badHttpRequestException.StatusCode),
                Type = nameof(ErrorType.RequestValidation),
                Detail = "Request could not be read while processing your request",
                Extensions = new Dictionary<string, object?>
                {
                    { "errors", errors }
                }
            }
        });
    }
}