using Microsoft.Extensions.Caching.Hybrid;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Menu;

public sealed record GetMenuQuery : IQuery<IReadOnlyList<MenuCategoryDto>>;

internal sealed class GetMenuQueryHandler(ICategoryRepository categoryRepository, HybridCache cache)
    : IQueryHandler<GetMenuQuery, IReadOnlyList<MenuCategoryDto>>
{
    public async Task<Result<IReadOnlyList<MenuCategoryDto>>> Handle(
        GetMenuQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<MenuCategoryDto> menu = await cache.GetOrCreateAsync(
            CacheKeys.For(query),
            query,
            GetMenuAsync,
            tags: [CacheTags.Menu],
            cancellationToken: cancellationToken);

        return Result.Success(menu);
    }

    // Only what a guest can order: active categories in menu order, without the ones that have no active items
    private async ValueTask<IReadOnlyList<MenuCategoryDto>> GetMenuAsync(
        GetMenuQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Category> categories =
            await categoryRepository.GetByStatusAsync(CategoryStatus.Active, cancellationToken);

        return categories
            .Select(category => category.ToMenuCategoryDto())
            .Where(category => category.Items.Count != 0)
            .ToList();
    }
}