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

internal sealed record GetDashboardLiveRequest([FromQuery] string TimeZone);

internal sealed record GetDashboardLiveResponse(DashboardLiveDto Data);

internal sealed class GetDashboardLiveEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/dashboard/live", HandleAsync)
            .WithName("GetDashboardLive")
            .WithTags("Dashboard")
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<Ok<GetDashboardLiveResponse>, ProblemHttpResult>> HandleAsync(
        [AsParameters] GetDashboardLiveRequest request,
        [FromServices] IValidator<GetDashboardLiveRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        GetDashboardLiveQuery query = new(TimeZoneInfo.FindSystemTimeZoneById(request.TimeZone));

        Result<DashboardLiveDto> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetDashboardLiveResponse(result.Value))
            : result.ToProblemDetails();
    }

    internal sealed class GetDashboardLiveRequestValidator : AbstractValidator<GetDashboardLiveRequest>
    {
        public GetDashboardLiveRequestValidator()
        {
            RuleFor(request => request.TimeZone)
                .MustBeValidTimeZone();
        }
    }
}