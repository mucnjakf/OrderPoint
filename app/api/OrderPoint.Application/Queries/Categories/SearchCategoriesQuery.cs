using Microsoft.Extensions.Caching.Hybrid;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Categories;

public sealed record SearchCategoriesQuery(string SearchQuery) : IQuery<IReadOnlyList<CategoryDto>>;

internal sealed class SearchCategoriesQueryHandler(ICategoryRepository categoryRepository, HybridCache cache)
    : IQueryHandler<SearchCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    public async Task<Result<IReadOnlyList<CategoryDto>>> Handle(
        SearchCategoriesQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<CategoryDto> categories = await cache.GetOrCreateAsync(
            CacheKeys.For(query),
            query,
            SearchCategoriesAsync,
            tags: [CacheTags.Categories],
            cancellationToken: cancellationToken);

        return Result.Success(categories);
    }

    private async ValueTask<IReadOnlyList<CategoryDto>> SearchCategoriesAsync(
        SearchCategoriesQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Category> categories = await categoryRepository
            .SearchAsync(query.SearchQuery, cancellationToken);

        return categories
            .Select(category => category.ToCategoryDto(category.Items.Count))
            .ToList();
    }
}