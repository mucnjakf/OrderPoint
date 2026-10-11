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
internal sealed record GetBusiestTimesRequest([FromQuery] string TimeZone);

/// <summary>The heat map data.</summary>
/// <param name="Data">One entry per local weekday and hour that had orders in the last 28 days.</param>
internal sealed record GetBusiestTimesResponse(IReadOnlyList<BusiestTimeDto> Data);

internal sealed class GetBusiestTimesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/dashboard/busiest-times", HandleAsync)
            .WithName("GetBusiestTimes")
            .WithTags("Dashboard")
            .WithSummary("Get the busiest times")
            .WithDescription("""
                The number of orders per local weekday and hour over the last 28 days, for a heat map. Takes no
                period.
                """)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<Ok<GetBusiestTimesResponse>, ProblemHttpResult>> HandleAsync(
        [AsParameters] GetBusiestTimesRequest request,
        [FromServices] IValidator<GetBusiestTimesRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        GetBusiestTimesQuery query = new(TimeZoneInfo.FindSystemTimeZoneById(request.TimeZone));

        Result<IReadOnlyList<BusiestTimeDto>> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetBusiestTimesResponse(result.Value))
            : result.ToProblemDetails();
    }

    internal sealed class GetBusiestTimesRequestValidator : AbstractValidator<GetBusiestTimesRequest>
    {
        public GetBusiestTimesRequestValidator()
        {
            RuleFor(request => request.TimeZone)
                .MustBeValidTimeZone();
        }
    }
}