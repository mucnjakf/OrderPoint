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
/// <param name="TimeZone">The IANA time zone of the bartender, e.g. `Europe/Zagreb`. Today starts at its local
/// midnight.</param>
internal sealed record GetShiftRequest([FromQuery] string TimeZone);

/// <summary>The bartender's shift so far.</summary>
/// <param name="Data">The figures.</param>
internal sealed record GetShiftResponse(ShiftDto Data);

internal sealed class GetShiftEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/shift", HandleAsync)
            .WithName("GetShift")
            .WithTags("Shift")
            .WithSummary("Get my shift")
            .WithDescription("""
                The signed-in bartender's figures for today: orders waiting for any bartender, the bartender's own
                accepted and active orders, and the orders they completed since local midnight with the average
                minutes from placing to completing them. Not cached; reload it after a status change.
                """)
            .RequireAuthorization(AuthorizationPolicies.Bartender);
    }

    private static async Task<Results<Ok<GetShiftResponse>, ProblemHttpResult>> HandleAsync(
        [AsParameters] GetShiftRequest request,
        [FromServices] IValidator<GetShiftRequest> validator,
        [FromServices] ISender sender,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        GetShiftQuery query = new(user.GetUserId(), TimeZoneInfo.FindSystemTimeZoneById(request.TimeZone));

        Result<ShiftDto> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetShiftResponse(result.Value))
            : result.ToProblemDetails();
    }

    internal sealed class GetShiftRequestValidator : AbstractValidator<GetShiftRequest>
    {
        public GetShiftRequestValidator()
        {
            RuleFor(request => request.TimeZone)
                .MustBeValidTimeZone();
        }
    }
}