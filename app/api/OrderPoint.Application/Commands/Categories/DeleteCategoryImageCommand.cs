using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Application.Storage;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Categories;

public sealed record DeleteCategoryImageCommand(Guid Id) : ICommand;

internal sealed class DeleteCategoryImageCommandHandler(
    ICategoryRepository categoryRepository,
    IImageStorage imageStorage,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteCategoryImageCommand>
{
    public async Task<Result> Handle(DeleteCategoryImageCommand command, CancellationToken cancellationToken)
    {
        Category? category = await categoryRepository.GetAsync(command.Id, cancellationToken);

        if (category is null)
        {
            return Result.Failure(CategoryErrors.NotFound);
        }

        if (category.ImageUrl is null)
        {
            return Result.Success();
        }

        string imageUrl = category.ImageUrl;

        category.RemoveImage();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await imageStorage.DeleteAsync(imageUrl, cancellationToken);

        return Result.Success();
    }
}