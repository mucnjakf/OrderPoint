using Microsoft.Extensions.Logging;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Items;

public sealed record ChangeItemsCategoryCommand(IReadOnlyList<Guid> Ids, Guid CategoryId)
    : ICommand, ICacheInvalidatingCommand, IMenuNotifyingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.ItemChanges;
}

internal sealed partial class ChangeItemsCategoryCommandHandler(
    IItemRepository itemRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    ILogger<ChangeItemsCategoryCommandHandler> logger)
    : ICommandHandler<ChangeItemsCategoryCommand>
{
    public async Task<Result> Handle(ChangeItemsCategoryCommand command, CancellationToken cancellationToken)
    {
        bool categoryExists = await categoryRepository.ExistsAsync(command.CategoryId, cancellationToken);

        if (!categoryExists)
        {
            return Result.Failure(CategoryErrors.NotFound);
        }

        IReadOnlyList<Item> items = await itemRepository.GetAsync(command.Ids, cancellationToken);

        if (items.Count != command.Ids.Count)
        {
            return Result.Failure(ItemErrors.NotFound);
        }

        foreach (Item item in items)
        {
            item.ChangeCategory(command.CategoryId);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        LogCategoryChanged(items.Count, command.CategoryId);

        return Result.Success();
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "{ItemCount} items moved to category {CategoryId}")]
    private partial void LogCategoryChanged(int itemCount, Guid categoryId);
}