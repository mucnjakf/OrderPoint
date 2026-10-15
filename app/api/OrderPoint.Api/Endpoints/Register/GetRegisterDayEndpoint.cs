using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Register;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Register;

/// <summary>The register's time zone.</summary>
/// <param name="TimeZone">The IANA time zone of the bar, e.g. `Europe/Zagreb`. Today starts at the business day's
/// start hour in it (e.g. 06:00).</param>
internal sealed record GetRegisterDayRequest([FromQuery] string TimeZone);

/// <summary>The register's day so far.</summary>
/// <param name="Data">The totals.</param>
internal sealed record GetRegisterDayResponse(RegisterDayDto Data);

internal sealed class GetRegisterDayEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/register/day", HandleAsync)
            .WithName("GetRegisterDay")
            .WithTags("Register")
            .WithSummary("Get the register's day")
            .WithDescription("""
                What was paid since the business day started: cash and card taken at the register (refunds not yet
                handed back included, since that money is still there), guests' card payments in the customer app,
                the same per bartender who took payments, and every refund still to hand back. Not cached; reload it
                after a payment.
                """)
            .RequireAuthorization(AuthorizationPolicies.Bartender);
    }

    private static async Task<Results<Ok<GetRegisterDayResponse>, ProblemHttpResult>> HandleAsync(
        [AsParameters] GetRegisterDayRequest request,
        [FromServices] IValidator<GetRegisterDayRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        GetRegisterDayQuery query = new(TimeZoneInfo.FindSystemTimeZoneById(request.TimeZone));

        Result<RegisterDayDto> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetRegisterDayResponse(result.Value))
            : result.ToProblemDetails();
    }

    internal sealed class GetRegisterDayRequestValidator : AbstractValidator<GetRegisterDayRequest>
    {
        public GetRegisterDayRequestValidator()
        {
            RuleFor(request => request.TimeZone)
                .MustBeValidTimeZone();
        }
    }
}