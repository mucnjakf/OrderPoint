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

internal sealed class GetCategoriesQueryHandler(ICategoryRepository categoryRepository, IItemRepository itemRepository)
    : IQueryHandler<GetCategoriesQuery, PaginationDto<CategoryDto>>
{
    public async Task<Result<PaginationDto<CategoryDto>>> Handle(
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