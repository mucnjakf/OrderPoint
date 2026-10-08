using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Orders;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;
using OrderPoint.Domain.Sorting;

namespace OrderPoint.Api.Endpoints.Orders;

internal sealed record GetOrdersRequest(
    [FromQuery] int PageNumber,
    [FromQuery] int PageSize,
    [FromQuery] string? SearchQuery,
    [FromQuery] OrderStatus? Status,
    [FromQuery] Guid? ItemId,
    [FromQuery] OrderSortBy? SortBy);

internal sealed record GetOrdersResponse(PaginationDto<OrderDto> Data);

internal sealed class GetOrdersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/orders", HandleAsync)
            .WithName("GetOrders")
            .WithTags("Orders");
    }

    private static async Task<Results<Ok<GetOrdersResponse>, ProblemHttpResult>> HandleAsync(
        [AsParameters] GetOrdersRequest request,
        [FromServices] IValidator<GetOrdersRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        GetOrdersQuery query = new(
            request.PageNumber,
            request.PageSize,
            request.SearchQuery,
            request.Status,
            request.ItemId,
            request.SortBy);

        Result<PaginationDto<OrderDto>> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetOrdersResponse(result.Value))
            : result.ToProblemDetails();
    }

    internal sealed class GetOrdersRequestValidator : AbstractValidator<GetOrdersRequest>
    {
        public GetOrdersRequestValidator()
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