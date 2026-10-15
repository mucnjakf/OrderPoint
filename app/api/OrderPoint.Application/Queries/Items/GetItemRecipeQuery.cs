using Microsoft.Extensions.Caching.Hybrid;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Items;

public sealed record GetItemRecipeQuery(Guid Id) : IQuery<ItemRecipeDto>;

internal sealed class GetItemRecipeQueryHandler(IItemRepository itemRepository, HybridCache cache)
    : IQueryHandler<GetItemRecipeQuery, ItemRecipeDto>
{
    public async Task<Result<ItemRecipeDto>> Handle(GetItemRecipeQuery query, CancellationToken cancellationToken)
    {
        ItemRecipeDto? recipe = await cache.GetOrCreateAsync(
            CacheKeys.For(query),
            query,
            GetItemRecipeAsync,
            tags: [CacheTags.Items],
            cancellationToken: cancellationToken);

        if (recipe is null)
        {
            return Result.Failure<ItemRecipeDto>(ItemErrors.NotFound);
        }

        return Result.Success(recipe);
    }

    private async ValueTask<ItemRecipeDto?> GetItemRecipeAsync(
        GetItemRecipeQuery query,
        CancellationToken cancellationToken)
    {
        Item? item = await itemRepository.GetAsync(query.Id, cancellationToken);

        return item?.ToItemRecipeDto(item.RecipeLines);
    }
}