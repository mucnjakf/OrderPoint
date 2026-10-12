using Microsoft.Extensions.Caching.Hybrid;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;
using OrderPoint.Domain.Sorting;

namespace OrderPoint.Application.Queries.Categories;

public sealed record GetCategoriesQuery(
    int PageNumber,
    int PageSize,
    string? SearchQuery,
    CategoryStatus? Status,
    CategorySortBy? SortBy)
    : IQuery<PaginationDto<CategoryDto>>;

internal sealed class GetCategoriesQueryHandler(ICategoryRepository categoryRepository, HybridCache cache)
    : IQueryHandler<GetCategoriesQuery, PaginationDto<CategoryDto>>
{
    public async Task<Result<PaginationDto<CategoryDto>>> Handle(
        GetCategoriesQuery query,
        CancellationToken cancellationToken)
    {
        PaginationDto<CategoryDto> categories = await cache.GetOrCreateAsync(
            CacheKeys.For(query),
            query,
            GetCategoriesAsync,
            tags: [CacheTags.Categories],
            cancellationToken: cancellationToken);

        return Result.Success(categories);
    }

    private async ValueTask<PaginationDto<CategoryDto>> GetCategoriesAsync(
        GetCategoriesQuery query,
        CancellationToken cancellationToken)
    {
        (IReadOnlyList<Category> categories, int totalCount) = await categoryRepository
            .GetPaginatedAsync(
                query.PageNumber,
                query.PageSize,
                query.SearchQuery,
                query.Status,
                query.SortBy,
                cancellationToken);

        return new PaginationDto<CategoryDto>(
            categories.Select(category => category.ToCategoryDto(category.Items.Count)).ToList(),
            query.PageNumber,
            query.PageSize,
            totalCount);
    }
}