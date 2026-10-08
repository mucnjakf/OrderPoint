using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Items;

public sealed record GetItemQuery(Guid Id) : IQuery<ItemDto>;

internal sealed class GetItemQueryHandler(IItemRepository itemRepository, IOrderRepository orderRepository)
    : IQueryHandler<GetItemQuery, ItemDto>
{
    public async Task<Result<ItemDto>> Handle(GetItemQuery query, CancellationToken cancellationToken)
    {
        Item? item = await itemRepository.GetAsync(query.Id, cancellationToken);

        if (item is null)
        {
            return Result.Failure<ItemDto>(ItemErrors.NotFound);
        }

        int ordersCount = await orderRepository.CountByItemAsync(item.Id, cancellationToken);

        var itemDto = item.ToItemDto(ordersCount);

        return Result.Success(itemDto);
    }
}