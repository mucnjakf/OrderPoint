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
internal sealed record GetBartenderLeaderboardRequest(
    [FromQuery] DashboardPeriod Period,
    [FromQuery] string TimeZone);

/// <summary>The leaderboard.</summary>
/// <param name="Data">Up to 5 bartenders, best first.</param>
internal sealed record GetBartenderLeaderboardResponse(IReadOnlyList<BartenderLeaderboardEntryDto> Data);

internal sealed class GetBartenderLeaderboardEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/dashboard/bartender-leaderboard", HandleAsync)
            .WithName("GetBartenderLeaderboard")
            .WithTags("Dashboard")
            .WithSummary("Get the bartender leaderboard")
            .WithDescription("""
                The 5 best bartenders of the period by completed revenue (then completed orders), with the number of
                orders they handled, completed and declined.
                """)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<Ok<GetBartenderLeaderboardResponse>, ProblemHttpResult>> HandleAsync(
        [AsParameters] GetBartenderLeaderboardRequest request,
        [FromServices] IValidator<GetBartenderLeaderboardRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        GetBartenderLeaderboardQuery query = new(request.Period, TimeZoneInfo.FindSystemTimeZoneById(request.TimeZone));

        Result<IReadOnlyList<BartenderLeaderboardEntryDto>> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetBartenderLeaderboardResponse(result.Value))
            : result.ToProblemDetails();
    }

    internal sealed class GetBartenderLeaderboardRequestValidator : AbstractValidator<GetBartenderLeaderboardRequest>
    {
        public GetBartenderLeaderboardRequestValidator()
        {
            RuleFor(request => request.Period)
                .IsInEnum().WithMessage("Period is invalid");

            RuleFor(request => request.TimeZone)
                .MustBeValidTimeZone();
        }
    }
}