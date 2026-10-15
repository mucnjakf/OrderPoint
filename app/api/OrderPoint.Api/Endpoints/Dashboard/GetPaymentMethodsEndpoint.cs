using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Dashboard;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Dashboard;

/// <summary>The period and the viewer's time zone.</summary>
/// <param name="Period">The period: `Today` (since the business day started, e.g. 06:00 local time), `Last7Days` or
/// `Last30Days` (both including today; every day starts at that hour).</param>
/// <param name="TimeZone">The IANA time zone of the viewer, e.g. `Europe/Zagreb`.</param>
internal sealed record GetPaymentMethodsRequest(
    [FromQuery] DashboardPeriod Period,
    [FromQuery] string TimeZone);

/// <summary>Revenue by payment method.</summary>
/// <param name="Data">Cash, card at the register and card online.</param>
internal sealed record GetPaymentMethodsResponse(PaymentMethodsDto Data);

internal sealed class GetPaymentMethodsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/dashboard/payment-methods", HandleAsync)
            .WithName("GetPaymentMethods")
            .WithTags("Dashboard")
            .WithSummary("Get revenue by payment method")
            .WithDescription("""
                What was paid in the period (by payment time, refunded payments not counted), split into cash at the
                register, card on the bar's terminal at the register and guests' cards in the customer app, with how
                many payments each.
                """)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<Ok<GetPaymentMethodsResponse>, ProblemHttpResult>> HandleAsync(
        [AsParameters] GetPaymentMethodsRequest request,
        [FromServices] IValidator<GetPaymentMethodsRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        GetPaymentMethodsQuery query = new(request.Period, TimeZoneInfo.FindSystemTimeZoneById(request.TimeZone));

        Result<PaymentMethodsDto> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetPaymentMethodsResponse(result.Value))
            : result.ToProblemDetails();
    }

    internal sealed class GetPaymentMethodsRequestValidator : AbstractValidator<GetPaymentMethodsRequest>
    {
        public GetPaymentMethodsRequestValidator()
        {
            RuleFor(request => request.Period)
                .IsInEnum().WithMessage("Period is invalid");

            RuleFor(request => request.TimeZone)
                .MustBeValidTimeZone();
        }
    }
}