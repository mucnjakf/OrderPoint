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

internal sealed record GetCategoryRevenueRequest([FromQuery] DashboardPeriod Period);

internal sealed record GetCategoryRevenueResponse(IReadOnlyList<CategoryRevenueDto> Data);

internal sealed class GetCategoryRevenueEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/dashboard/category-revenue", HandleAsync)
            .WithName("GetCategoryRevenue")
            .WithTags("Dashboard")
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<Ok<GetCategoryRevenueResponse>, ProblemHttpResult>> HandleAsync(
        [AsParameters] GetCategoryRevenueRequest request,
        [FromServices] IValidator<GetCategoryRevenueRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        GetCategoryRevenueQuery query = new(request.Period);

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
        }
    }
}