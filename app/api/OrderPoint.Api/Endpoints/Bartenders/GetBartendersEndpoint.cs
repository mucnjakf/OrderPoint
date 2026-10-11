using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Bartenders;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;
using OrderPoint.Domain.Sorting;

namespace OrderPoint.Api.Endpoints.Bartenders;

/// <summary>Paging, search, filter and sort options.</summary>
/// <param name="PageNumber">The page to return, starting at 1.</param>
/// <param name="PageSize">The number of bartenders per page.</param>
/// <param name="SearchQuery">Matches the first or the last name, ignoring case.</param>
/// <param name="Status">Only bartenders with this status.</param>
/// <param name="SortBy">The sort order. Default: `CreatedAtUtcDesc` (newest first).</param>
internal sealed record GetBartendersRequest(
    [FromQuery] int PageNumber,
    [FromQuery] int PageSize,
    [FromQuery] string? SearchQuery,
    [FromQuery] BartenderStatus? Status,
    [FromQuery] BartenderSortBy? SortBy);

/// <summary>A page of bartenders.</summary>
/// <param name="Data">The page.</param>
internal sealed record GetBartendersResponse(PaginationDto<BartenderDto> Data);

internal sealed class GetBartendersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/bartenders", HandleAsync)
            .WithName("GetBartenders")
            .WithTags("Bartenders")
            .WithSummary("List bartenders")
            .WithDescription("""
                A page of bartenders with search (first or last name), a status filter and sorting (default: newest
                first). `ordersCount` is the number of orders the bartender handled.
                """)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<Ok<GetBartendersResponse>, ProblemHttpResult>> HandleAsync(
        [AsParameters] GetBartendersRequest request,
        [FromServices] IValidator<GetBartendersRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        GetBartendersQuery query = new(
            request.PageNumber,
            request.PageSize,
            request.SearchQuery,
            request.Status,
            request.SortBy);

        Result<PaginationDto<BartenderDto>> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetBartendersResponse(result.Value))
            : result.ToProblemDetails();
    }

    internal sealed class GetBartendersRequestValidator : AbstractValidator<GetBartendersRequest>
    {
        public GetBartendersRequestValidator()
        {
            RuleFor(request => request.PageNumber)
                .GreaterThan(0).WithMessage("PageNumber must be positive");

            RuleFor(request => request.PageSize)
                .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100");

            RuleFor(request => request.SearchQuery)
                .MaximumLength(100).WithMessage("SearchQuery must be at most 100 characters");

            RuleFor(request => request.Status)
                .IsInEnum().WithMessage("Status is invalid");

            RuleFor(request => request.SortBy)
                .IsInEnum().WithMessage("SortBy is invalid");
        }
    }
}