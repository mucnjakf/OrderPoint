using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Outcomes;
using OrderPoint.Domain.Sorting;

namespace OrderPoint.Application.Queries.Items;

public sealed record GetItemsQuery(
    int PageNumber,
    int PageSize,
    string? SearchQuery,
    Guid? CategoryId,
    ItemSortBy? SortBy)
    : IQuery<PaginationDto<ItemDto>>;

internal sealed class GetItemsQueryHandler(IItemRepository itemRepository, IOrderRepository orderRepository)
    : IQueryHandler<GetItemsQuery, PaginationDto<ItemDto>>
{
    public async Task<Result<PaginationDto<ItemDto>>> Handle(
        GetItemsQuery query,
        CancellationToken cancellationToken)
    {
        (IReadOnlyList<Item> items, int totalCount) = await itemRepository
            .GetPaginatedAsync(
                query.PageNumber,
                query.PageSize,
                query.SearchQuery,
                query.CategoryId,
                query.SortBy,
                cancellationToken);

        IReadOnlyDictionary<Guid, int> ordersCounts = await orderRepository.CountAsync(
            items.Select(item => item.Id).ToList(),
            cancellationToken);

        return new PaginationDto<ItemDto>(
            items.Select(item => item.ToItemDto(ordersCounts.GetValueOrDefault(item.Id))).ToList(),
            query.PageNumber,
            query.PageSize,
            totalCount);
    }
}