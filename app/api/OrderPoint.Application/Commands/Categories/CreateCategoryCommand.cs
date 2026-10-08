using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Categories;

public sealed record CreateCategoryCommand(
    string Name,
    string Description,
    CategoryStatus Status)
    : ICommand<CategoryDto>;

internal sealed class CreateCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateCategoryCommand, CategoryDto>
{
    public async Task<Result<CategoryDto>> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        Result<Category> result = Category.Create(command.Name, command.Description, command.Status);

        if (result.IsFailure)
        {
            return Result.Failure<CategoryDto>(result.Error);
        }

        await categoryRepository.CreateAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var categoryDto = result.Value.ToCategoryDto(itemsCount: 0);

        return Result.Success(categoryDto);
    }
}