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

internal sealed record GetRevenueTrendRequest([FromQuery] DashboardPeriod Period);

internal sealed record GetRevenueTrendResponse(IReadOnlyList<RevenueTrendPointDto> Data);

internal sealed class GetRevenueTrendEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/dashboard/revenue-trend", HandleAsync)
            .WithName("GetRevenueTrend")
            .WithTags("Dashboard")
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<Ok<GetRevenueTrendResponse>, ProblemHttpResult>> HandleAsync(
        [AsParameters] GetRevenueTrendRequest request,
        [FromServices] IValidator<GetRevenueTrendRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        GetRevenueTrendQuery query = new(request.Period);

        Result<IReadOnlyList<RevenueTrendPointDto>> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetRevenueTrendResponse(result.Value))
            : result.ToProblemDetails();
    }

    internal sealed class GetRevenueTrendRequestValidator : AbstractValidator<GetRevenueTrendRequest>
    {
        public GetRevenueTrendRequestValidator()
        {
            RuleFor(request => request.Period)
                .IsInEnum().WithMessage("Period is invalid");
        }
    }
}