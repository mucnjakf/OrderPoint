using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Dashboard;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Dashboard;

internal sealed record GetBusiestTimesRequest([FromQuery] string TimeZone);

internal sealed record GetBusiestTimesResponse(IReadOnlyList<BusiestTimeDto> Data);

internal sealed class GetBusiestTimesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/dashboard/busiest-times", HandleAsync)
            .WithName("GetBusiestTimes")
            .WithTags("Dashboard")
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<Ok<GetBusiestTimesResponse>, ProblemHttpResult>> HandleAsync(
        [AsParameters] GetBusiestTimesRequest request,
        [FromServices] IValidator<GetBusiestTimesRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        GetBusiestTimesQuery query = new(TimeZoneInfo.FindSystemTimeZoneById(request.TimeZone));

        Result<IReadOnlyList<BusiestTimeDto>> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetBusiestTimesResponse(result.Value))
            : result.ToProblemDetails();
    }

    internal sealed class GetBusiestTimesRequestValidator : AbstractValidator<GetBusiestTimesRequest>
    {
        public GetBusiestTimesRequestValidator()
        {
            RuleFor(request => request.TimeZone)
                .MustBeValidTimeZone();
        }
    }
}