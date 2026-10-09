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

internal sealed record GetTopItemsRequest(
    [FromQuery] DashboardPeriod Period,
    [FromQuery] string TimeZone);

internal sealed record GetTopItemsResponse(IReadOnlyList<TopItemDto> Data);

internal sealed class GetTopItemsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/dashboard/top-items", HandleAsync)
            .WithName("GetTopItems")
            .WithTags("Dashboard")
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