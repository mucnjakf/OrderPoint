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

internal sealed record GetDashboardSummaryRequest([FromQuery] DashboardPeriod Period);

internal sealed record GetDashboardSummaryResponse(DashboardSummaryDto Data);

internal sealed class GetDashboardSummaryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/dashboard/summary", HandleAsync)
            .WithName("GetDashboardSummary")
            .WithTags("Dashboard");
    }

    private static async Task<Results<Ok<GetDashboardSummaryResponse>, ProblemHttpResult>> HandleAsync(
        [AsParameters] GetDashboardSummaryRequest request,
        [FromServices] IValidator<GetDashboardSummaryRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        GetDashboardSummaryQuery query = new(request.Period);

        Result<DashboardSummaryDto> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetDashboardSummaryResponse(result.Value))
            : result.ToProblemDetails();
    }

    internal sealed class GetDashboardSummaryRequestValidator : AbstractValidator<GetDashboardSummaryRequest>
    {
        public GetDashboardSummaryRequestValidator()
        {
            RuleFor(request => request.Period)
                .IsInEnum().WithMessage("Period is invalid");
        }
    }
}