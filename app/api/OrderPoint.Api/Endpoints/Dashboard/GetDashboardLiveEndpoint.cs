using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Dashboard;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Dashboard;

/// <summary>The viewer's time zone.</summary>
/// <param name="TimeZone">The IANA time zone of the viewer, e.g. `Europe/Zagreb`. Periods, buckets and weekdays
/// follow its local calendar.</param>
internal sealed record GetDashboardLiveRequest([FromQuery] string TimeZone);

/// <summary>The live figures.</summary>
/// <param name="Data">The figures.</param>
internal sealed record GetDashboardLiveResponse(DashboardLiveDto Data);

internal sealed class GetDashboardLiveEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/dashboard/live", HandleAsync)
            .WithName("GetDashboardLive")
            .WithTags("Dashboard")
            .WithSummary("Get the live figures")
            .WithDescription("""
                Open orders by status, the oldest open order, today's figures since the business day started (orders,
                completed orders, what was paid, the average payment) and what waits at the register from any day
                (orders still to pay, refunds due). Meant to be polled; the Admin refreshes every 30 seconds.
                """)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<Ok<GetDashboardLiveResponse>, ProblemHttpResult>> HandleAsync(
        [AsParameters] GetDashboardLiveRequest request,
        [FromServices] IValidator<GetDashboardLiveRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        GetDashboardLiveQuery query = new(TimeZoneInfo.FindSystemTimeZoneById(request.TimeZone));

        Result<DashboardLiveDto> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetDashboardLiveResponse(result.Value))
            : result.ToProblemDetails();
    }

    internal sealed class GetDashboardLiveRequestValidator : AbstractValidator<GetDashboardLiveRequest>
    {
        public GetDashboardLiveRequestValidator()
        {
            RuleFor(request => request.TimeZone)
                .MustBeValidTimeZone();
        }
    }
}