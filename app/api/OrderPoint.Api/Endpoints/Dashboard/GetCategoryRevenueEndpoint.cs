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
internal sealed record GetCategoryRevenueRequest(
    [FromQuery] DashboardPeriod Period,
    [FromQuery] string TimeZone);

/// <summary>Revenue per category.</summary>
/// <param name="Data">Up to 5 categories, highest revenue first.</param>
internal sealed record GetCategoryRevenueResponse(IReadOnlyList<CategoryRevenueDto> Data);

internal sealed class GetCategoryRevenueEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/dashboard/category-revenue", HandleAsync)
            .WithName("GetCategoryRevenue")
            .WithTags("Dashboard")
            .WithSummary("Get revenue by category")
            .WithDescription("""
                Completed revenue per category for the period: the top 5 categories, highest first.
                """)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<Ok<GetCategoryRevenueResponse>, ProblemHttpResult>> HandleAsync(
        [AsParameters] GetCategoryRevenueRequest request,
        [FromServices] IValidator<GetCategoryRevenueRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        GetCategoryRevenueQuery query = new(request.Period, TimeZoneInfo.FindSystemTimeZoneById(request.TimeZone));

        Result<IReadOnlyList<CategoryRevenueDto>> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetCategoryRevenueResponse(result.Value))
            : result.ToProblemDetails();
    }

    internal sealed class GetCategoryRevenueRequestValidator : AbstractValidator<GetCategoryRevenueRequest>
    {
        public GetCategoryRevenueRequestValidator()
        {
            RuleFor(request => request.Period)
                .IsInEnum().WithMessage("Period is invalid");

            RuleFor(request => request.TimeZone)
                .MustBeValidTimeZone();
        }
    }
}