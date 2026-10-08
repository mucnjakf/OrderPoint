using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Items;
using OrderPoint.Domain.Outcomes;
using OrderPoint.Domain.Sorting;

namespace OrderPoint.Api.Endpoints.Items;

internal sealed record GetItemsRequest(
    [FromQuery] int PageNumber,
    [FromQuery] int PageSize,
    [FromQuery] string? SearchQuery,
    [FromQuery] Guid? CategoryId,
    [FromQuery] ItemSortBy? SortBy);

internal sealed record GetItemsResponse(PaginationDto<ItemDto> Data);

internal sealed class GetItemsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/items", HandleAsync)
            .WithName("GetItems")
            .WithTags("Items")
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<Ok<GetItemsResponse>, ProblemHttpResult>> HandleAsync(
        [AsParameters] GetItemsRequest request,
        [FromServices] IValidator<GetItemsRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        GetItemsQuery query = new(
            request.PageNumber,
            request.PageSize,
            request.SearchQuery,
            request.CategoryId,
            request.SortBy);

        Result<PaginationDto<ItemDto>> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetItemsResponse(result.Value))
            : result.ToProblemDetails();
    }

    internal sealed class GetItemsRequestValidator : AbstractValidator<GetItemsRequest>
    {
        public GetItemsRequestValidator()
        {
            RuleFor(request => request.PageNumber)
                .GreaterThan(0).WithMessage("PageNumber must be positive");

            RuleFor(request => request.PageSize)
                .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100");

            RuleFor(request => request.SearchQuery)
                .MaximumLength(100).WithMessage("SearchQuery must be at most 100 characters");

            RuleFor(request => request.SortBy)
                .IsInEnum().WithMessage("SortBy is invalid");
        }
    }
}