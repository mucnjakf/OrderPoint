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
/// `Last30Days` (both including today; every day starts at that hour). Previous-period comparisons use the window of
/// the same length right before it.</param>
/// <param name="TimeZone">The IANA time zone of the viewer, e.g. `Europe/Zagreb`. Periods, buckets and weekdays
/// follow its local calendar.</param>
internal sealed record GetRevenueTrendRequest(
    [FromQuery] DashboardPeriod Period,
    [FromQuery] string TimeZone);

/// <summary>The revenue trend.</summary>
/// <param name="Data">One point per hour (`Today`) or per day, oldest first.</param>
internal sealed record GetRevenueTrendResponse(IReadOnlyList<RevenueTrendPointDto> Data);

internal sealed class GetRevenueTrendEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/dashboard/revenue-trend", HandleAsync)
            .WithName("GetRevenueTrend")
            .WithTags("Dashboard")
            .WithSummary("Get the revenue trend")
            .WithDescription("""
                What was paid per bucket (by payment time) in the viewer's time zone: hourly for `Today`, daily for
                `Last7Days` and `Last30Days`. Buckets without revenue are included with 0.
                """)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<Ok<GetRevenueTrendResponse>, ProblemHttpResult>> HandleAsync(
        [AsParameters] GetRevenueTrendRequest request,
        [FromServices] IValidator<GetRevenueTrendRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        GetRevenueTrendQuery query = new(request.Period, TimeZoneInfo.FindSystemTimeZoneById(request.TimeZone));

        Result<IReadOnlyList<RevenueTrendPointDto>> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetRevenueTrendResponse(result.Value))
            : result.ToProblemDetails();
    }

    internal sealed class GetRevenueTrendRequestValidator : AbstractValidator<GetRevenueTrendRequest>
    {
        public GetRevenueTrendRequestValidator()
        {
            RuleFor(request => request.Period)
                .IsInEnum().WithMessage("Period is invalid");

            RuleFor(request => request.TimeZone)
                .MustBeValidTimeZone();
        }
    }
}