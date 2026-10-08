using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Application.Storage;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Categories;

public sealed record DeleteCategoryCommand(Guid Id) : ICommand;

internal sealed class DeleteCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IItemRepository itemRepository,
    IImageStorage imageStorage,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteCategoryCommand>
{
    public async Task<Result> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken)
    {
        Category? category = await categoryRepository.GetAsync(command.Id, cancellationToken);

        if (category is null)
        {
            return Result.Failure(CategoryErrors.NotFound);
        }

        bool containsItems = await itemRepository.ExistsByCategoryAsync(category.Id, cancellationToken);

        if (containsItems)
        {
            return Result.Failure(CategoryErrors.CannotDeleteCategoryWithItems);
        }

        categoryRepository.Delete(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (category.ImageUrl is not null)
        {
            await imageStorage.DeleteAsync(category.ImageUrl, cancellationToken);
        }

        return Result.Success();
    }
}