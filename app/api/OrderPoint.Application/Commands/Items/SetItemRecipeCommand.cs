using Microsoft.Extensions.Logging;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Items;

public sealed record SetItemRecipeLineCommand(Guid StockItemId, decimal Quantity);

// Replaces the whole recipe; an empty one stops tracking the item's stock
public sealed record SetItemRecipeCommand(Guid Id, IReadOnlyList<SetItemRecipeLineCommand> Lines)
    : ICommand, ICacheInvalidatingCommand, IMenuNotifyingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.ItemChanges;
}

internal sealed partial class SetItemRecipeCommandHandler(
    IItemRepository itemRepository,
    IStockItemRepository stockItemRepository,
    IRecipeLineRepository recipeLineRepository,
    IUnitOfWork unitOfWork,
    ILogger<SetItemRecipeCommandHandler> logger)
    : ICommandHandler<SetItemRecipeCommand>
{
    public async Task<Result> Handle(SetItemRecipeCommand command, CancellationToken cancellationToken)
    {
        Item? item = await itemRepository.GetAsync(command.Id, cancellationToken);

        if (item is null)
        {
            return Result.Failure(ItemErrors.NotFound);
        }

        List<Guid> stockItemIds = command.Lines.Select(line => line.StockItemId).Distinct().ToList();

        IReadOnlyList<StockItem> stockItems = await stockItemRepository.GetAsync(stockItemIds, cancellationToken);

        Dictionary<Guid, StockItem> stockItemsById = stockItems.ToDictionary(stockItem => stockItem.Id);

        if (stockItemIds.Any(stockItemId => !stockItemsById.ContainsKey(stockItemId)))
        {
            return Result.Failure(StockItemErrors.NotFound);
        }

        Result<IReadOnlyList<RecipeLine>> result = item.CreateRecipe(command.Lines
            .Select(line => (stockItemsById[line.StockItemId], line.Quantity))
            .ToList());

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        foreach (RecipeLine recipeLine in item.RecipeLines)
        {
            recipeLineRepository.Delete(recipeLine);
        }

        foreach (RecipeLine recipeLine in result.Value)
        {
            await recipeLineRepository.CreateAsync(recipeLine, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        LogRecipeSet(item.Id, item.Name, result.Value.Count);

        return Result.Success();
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Recipe of item {ItemId} ({Name}) set: {LineCount} ingredients")]
    private partial void LogRecipeSet(Guid itemId, string name, int lineCount);
}