using Microsoft.Extensions.Caching.Hybrid;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Categories;

public sealed record GetCategoryQuery(Guid Id) : IQuery<CategoryDto>;

internal sealed class GetCategoryQueryHandler(
    ICategoryRepository categoryRepository,
    IItemRepository itemRepository,
    HybridCache cache)
    : IQueryHandler<GetCategoryQuery, CategoryDto>
{
    public async Task<Result<CategoryDto>> Handle(GetCategoryQuery query, CancellationToken cancellationToken)
    {
        CategoryDto? categoryDto = await cache.GetOrCreateAsync(
            CacheKeys.For(query),
            query,
            GetCategoryAsync,
            tags: [CacheTags.Categories],
            cancellationToken: cancellationToken);

        if (categoryDto is null)
        {
            return Result.Failure<CategoryDto>(CategoryErrors.NotFound);
        }

        return Result.Success(categoryDto);
    }

    private async ValueTask<CategoryDto?> GetCategoryAsync(GetCategoryQuery query, CancellationToken cancellationToken)
    {
        Category? category = await categoryRepository.GetAsync(query.Id, cancellationToken);

        if (category is null)
        {
            return null;
        }

        int itemsCount = await itemRepository.CountByCategoryAsync(category.Id, cancellationToken);

        return category.ToCategoryDto(itemsCount);
    }
}