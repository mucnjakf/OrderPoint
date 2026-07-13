using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Categories;

public sealed record GetCategoryQuery(Guid CategoryId) : IQuery<CategoryDto>;

internal sealed class GetCategoryQueryHandler(ICategoryRepository categoryRepository, IItemRepository itemRepository)
    : IQueryHandler<GetCategoryQuery, CategoryDto>
{
    public async Task<Result<CategoryDto>> Handle(GetCategoryQuery query, CancellationToken cancellationToken)
    {
        Category? category = await categoryRepository.GetAsync(query.CategoryId, cancellationToken);

        if (category is null)
        {
            return Result.Failure<CategoryDto>(CategoryErrors.NotFound);
        }

        int itemsCount = await itemRepository.CountAsync(category.Id, cancellationToken);

        var categoryDto = category.ToCategoryDto(itemsCount);

        return Result.Success(categoryDto);
    }
}