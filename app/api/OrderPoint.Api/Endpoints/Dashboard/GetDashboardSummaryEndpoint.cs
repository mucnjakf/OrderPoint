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
/// <param name="Period">The period: `Today` (since local midnight), `Last7Days` or `Last30Days` (both including
/// today). Previous-period comparisons use the window of the same length right before it.</param>
/// <param name="TimeZone">The IANA time zone of the viewer, e.g. `Europe/Zagreb`. Periods, buckets and weekdays
/// follow its local calendar.</param>
internal sealed record GetDashboardSummaryRequest(
    [FromQuery] DashboardPeriod Period,
    [FromQuery] string TimeZone);

/// <summary>The key figures.</summary>
/// <param name="Data">The figures with the previous period.</param>
internal sealed record GetDashboardSummaryResponse(DashboardSummaryDto Data);

internal sealed class GetDashboardSummaryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/dashboard/summary", HandleAsync)
            .WithName("GetDashboardSummary")
            .WithTags("Dashboard")
            .WithSummary("Get the key figures")
            .WithDescription("""
                Revenue, number of orders, average order value and average service time for the period, each with the
                value of the previous period of the same length. Revenue and averages count completed orders only; the
                number of orders includes every status.
                """)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<Ok<GetDashboardSummaryResponse>, ProblemHttpResult>> HandleAsync(
        [AsParameters] GetDashboardSummaryRequest request,
        [FromServices] IValidator<GetDashboardSummaryRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        GetDashboardSummaryQuery query = new(request.Period, TimeZoneInfo.FindSystemTimeZoneById(request.TimeZone));

        Result<DashboardSummaryDto> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetDashboardSummaryResponse(result.Value))
            : result.ToProblemDetails();
    }

    internal sealed class GetDashboardSummaryRequestValidator : AbstractValidator<GetDashboardSummaryRequest>
    {
        public GetDashboardSummaryRequestValidator()
        {
            RuleFor(request => request.Period)
                .IsInEnum().WithMessage("Period is invalid");

            RuleFor(request => request.TimeZone)
                .MustBeValidTimeZone();
        }
    }
}