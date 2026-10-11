using Microsoft.Extensions.Logging;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Application.Storage;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Categories;

public sealed record DeleteCategoriesCommand(IReadOnlyList<Guid> Ids) : ICommand;

internal sealed partial class DeleteCategoriesCommandHandler(
    ICategoryRepository categoryRepository,
    IItemRepository itemRepository,
    IImageStorage imageStorage,
    IUnitOfWork unitOfWork,
    ILogger<DeleteCategoriesCommandHandler> logger)
    : ICommandHandler<DeleteCategoriesCommand>
{
    public async Task<Result> Handle(DeleteCategoriesCommand command, CancellationToken cancellationToken)
    {
        IReadOnlyList<Category> categories = await categoryRepository.GetAsync(command.Ids, cancellationToken);

        if (categories.Count != command.Ids.Count)
        {
            return Result.Failure(CategoryErrors.NotFound);
        }

        bool containItems = await itemRepository.ExistsByCategoriesAsync(command.Ids, cancellationToken);

        if (containItems)
        {
            return Result.Failure(CategoryErrors.CannotDeleteCategoryWithItems);
        }

        foreach (Category category in categories)
        {
            categoryRepository.Delete(category);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (Category category in categories.Where(category => category.ImageUrl is not null))
        {
            await imageStorage.DeleteAsync(category.ImageUrl!, cancellationToken);
        }

        LogCategoriesDeleted(categories.Count);

        return Result.Success();
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "{CategoryCount} categories deleted")]
    private partial void LogCategoriesDeleted(int categoryCount);
}