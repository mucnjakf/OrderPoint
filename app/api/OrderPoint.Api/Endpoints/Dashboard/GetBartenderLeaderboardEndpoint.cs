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

internal sealed record GetBartenderLeaderboardRequest([FromQuery] DashboardPeriod Period);

internal sealed record GetBartenderLeaderboardResponse(IReadOnlyList<BartenderLeaderboardEntryDto> Data);

internal sealed class GetBartenderLeaderboardEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/dashboard/bartender-leaderboard", HandleAsync)
            .WithName("GetBartenderLeaderboard")
            .WithTags("Dashboard");
    }

    private static async Task<Results<Ok<GetBartenderLeaderboardResponse>, ProblemHttpResult>> HandleAsync(
        [AsParameters] GetBartenderLeaderboardRequest request,
        [FromServices] IValidator<GetBartenderLeaderboardRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        GetBartenderLeaderboardQuery query = new(request.Period);

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
        }
    }
}