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
internal sealed record GetGrossProfitRequest(
    [FromQuery] DashboardPeriod Period,
    [FromQuery] string TimeZone);

/// <summary>The period's gross profit and stock losses.</summary>
/// <param name="Data">Revenue, cost of goods, gross profit, and the value of waste and count differences.</param>
internal sealed record GetGrossProfitResponse(GrossProfitDto Data);

internal sealed class GetGrossProfitEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/dashboard/gross-profit", HandleAsync)
            .WithName("GetGrossProfit")
            .WithTags("Dashboard")
            .WithSummary("Get the gross profit")
            .WithDescription("""
                What was paid in the period (by payment time, like the summary's revenue) minus the cost of goods:
                the `Sale` stock movements of the period (written when an order is completed), each valued at the
                unit cost stored on it. Revenue and cost are dated differently (payment and completion), which
                evens out over a period.

                - Items without a recipe have no cost, so `CostedRevenue` says how much of the revenue comes from
                  items that have one (by their recipe now).
                - `WasteValue` is the value of the period's waste; `CountDifferenceValue` is what stock counts and
                  corrections changed (negative for a loss).
                - `PreviousGrossProfit` covers the same length of time right before the period.
                """)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<Ok<GetGrossProfitResponse>, ProblemHttpResult>> HandleAsync(
        [AsParameters] GetGrossProfitRequest request,
        [FromServices] IValidator<GetGrossProfitRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        GetGrossProfitQuery query = new(request.Period, TimeZoneInfo.FindSystemTimeZoneById(request.TimeZone));

        Result<GrossProfitDto> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetGrossProfitResponse(result.Value))
            : result.ToProblemDetails();
    }

    internal sealed class GetGrossProfitRequestValidator : AbstractValidator<GetGrossProfitRequest>
    {
        public GetGrossProfitRequestValidator()
        {
            RuleFor(request => request.Period)
                .IsInEnum().WithMessage("Period is invalid");

            RuleFor(request => request.TimeZone)
                .MustBeValidTimeZone();
        }
    }
}