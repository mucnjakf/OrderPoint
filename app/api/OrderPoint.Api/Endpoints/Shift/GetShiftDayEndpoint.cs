using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Shift;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Shift;

/// <summary>The bartender's time zone.</summary>
/// <param name="TimeZone">The IANA time zone of the bartender, e.g. `Europe/Zagreb`. Today starts at the business day's
/// start hour in it (e.g. 06:00) and the hours are local hours.</param>
internal sealed record GetShiftDayRequest([FromQuery] string TimeZone);

/// <summary>What the bartender served today.</summary>
/// <param name="Data">The day.</param>
internal sealed record GetShiftDayResponse(ShiftDayDto Data);

internal sealed class GetShiftDayEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/shift/day", HandleAsync)
            .WithName("GetShiftDay")
            .WithTags("Shift")
            .WithSummary("Get my day")
            .WithDescription("""
                What the signed-in bartender served since the business day started: the total and the number of
                pieces of the orders they completed, completed orders per local hour (from the first hour with a
                completed order up to the current hour, counted from the day start, quiet hours included with 0), and
                the 3 items they served most. Orders count by when they were completed. Not cached; reload it after a
                status change.
                """)
            .RequireAuthorization(AuthorizationPolicies.Bartender);
    }

    private static async Task<Results<Ok<GetShiftDayResponse>, ProblemHttpResult>> HandleAsync(
        [AsParameters] GetShiftDayRequest request,
        [FromServices] IValidator<GetShiftDayRequest> validator,
        [FromServices] ISender sender,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        GetShiftDayQuery query = new(user.GetUserId(), TimeZoneInfo.FindSystemTimeZoneById(request.TimeZone));

        Result<ShiftDayDto> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetShiftDayResponse(result.Value))
            : result.ToProblemDetails();
    }

    internal sealed class GetShiftDayRequestValidator : AbstractValidator<GetShiftDayRequest>
    {
        public GetShiftDayRequestValidator()
        {
            RuleFor(request => request.TimeZone)
                .MustBeValidTimeZone();
        }
    }
}