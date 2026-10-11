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
internal sealed record GetTopItemsRequest(
    [FromQuery] DashboardPeriod Period,
    [FromQuery] string TimeZone);

/// <summary>The best-selling items.</summary>
/// <param name="Data">Up to 5 items, best-selling first.</param>
internal sealed record GetTopItemsResponse(IReadOnlyList<TopItemDto> Data);

internal sealed class GetTopItemsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/dashboard/top-items", HandleAsync)
            .WithName("GetTopItems")
            .WithTags("Dashboard")
            .WithSummary("Get the best-selling items")
            .WithDescription("""
                The 5 best-selling items of the period by quantity (then revenue), from completed orders.
                """)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<Ok<GetTopItemsResponse>, ProblemHttpResult>> HandleAsync(
        [AsParameters] GetTopItemsRequest request,
        [FromServices] IValidator<GetTopItemsRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        GetTopItemsQuery query = new(request.Period, TimeZoneInfo.FindSystemTimeZoneById(request.TimeZone));

        Result<IReadOnlyList<TopItemDto>> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetTopItemsResponse(result.Value))
            : result.ToProblemDetails();
    }

    internal sealed class GetTopItemsRequestValidator : AbstractValidator<GetTopItemsRequest>
    {
        public GetTopItemsRequestValidator()
        {
            RuleFor(request => request.Period)
                .IsInEnum().WithMessage("Period is invalid");

            RuleFor(request => request.TimeZone)
                .MustBeValidTimeZone();
        }
    }
}