using Microsoft.Extensions.Logging;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Application.Storage;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Items;

public sealed record DeleteItemCommand(Guid Id) : ICommand, ICacheInvalidatingCommand, IMenuNotifyingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.ItemChanges;
}

internal sealed partial class DeleteItemCommandHandler(
    IItemRepository itemRepository,
    IOrderRepository orderRepository,
    IRecipeLineRepository recipeLineRepository,
    IImageStorage imageStorage,
    IUnitOfWork unitOfWork,
    ILogger<DeleteItemCommandHandler> logger)
    : ICommandHandler<DeleteItemCommand>
{
    public async Task<Result> Handle(DeleteItemCommand command, CancellationToken cancellationToken)
    {
        Item? item = await itemRepository.GetAsync(command.Id, cancellationToken);

        if (item is null)
        {
            return Result.Failure(ItemErrors.NotFound);
        }

        bool hasOrders = await orderRepository.ExistsByItemAsync(item.Id, cancellationToken);

        if (hasOrders)
        {
            return Result.Failure(ItemErrors.CannotDeleteItemWithOrders);
        }

        // The recipe only describes the item, so it goes with it
        foreach (RecipeLine recipeLine in item.RecipeLines)
        {
            recipeLineRepository.Delete(recipeLine);
        }

        itemRepository.Delete(item);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (item.ImageUrl is not null)
        {
            await imageStorage.DeleteAsync(item.ImageUrl, cancellationToken);
        }

        LogItemDeleted(item.Id, item.Name);

        return Result.Success();
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Item {ItemId} ({Name}) deleted")]
    private partial void LogItemDeleted(Guid itemId, string name);
}