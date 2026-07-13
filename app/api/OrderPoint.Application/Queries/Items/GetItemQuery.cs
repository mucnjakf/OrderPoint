using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Items;

public sealed record GetItemQuery(Guid ItemId) : IQuery<ItemDto>;

internal sealed class GetItemQueryHandler(IItemRepository itemRepository)
    : IQueryHandler<GetItemQuery, ItemDto>
{
    public async Task<Result<ItemDto>> Handle(GetItemQuery query, CancellationToken cancellationToken)
    {
        Item? item = await itemRepository.GetAsync(query.ItemId, cancellationToken);

        if (item is null)
        {
            return Result.Failure<ItemDto>(ItemErrors.NotFound);
        }

        var itemDto = item.ToItemDto();

        return Result.Success(itemDto);
    }
}