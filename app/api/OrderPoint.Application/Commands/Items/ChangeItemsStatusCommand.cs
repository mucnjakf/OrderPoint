using Microsoft.Extensions.Logging;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Items;

public sealed record ChangeItemsStatusCommand(IReadOnlyList<Guid> Ids, ItemStatus Status)
    : ICommand, ICacheInvalidatingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.ItemChanges;
}

internal sealed partial class ChangeItemsStatusCommandHandler(
    IItemRepository itemRepository,
    IUnitOfWork unitOfWork,
    ILogger<ChangeItemsStatusCommandHandler> logger)
    : ICommandHandler<ChangeItemsStatusCommand>
{
    public async Task<Result> Handle(ChangeItemsStatusCommand command, CancellationToken cancellationToken)
    {
        IReadOnlyList<Item> items = await itemRepository.GetAsync(command.Ids, cancellationToken);

        if (items.Count != command.Ids.Count)
        {
            return Result.Failure(ItemErrors.NotFound);
        }

        foreach (Item item in items)
        {
            item.ChangeStatus(command.Status);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        LogStatusChanged(items.Count, command.Status);

        return Result.Success();
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "{ItemCount} items set to {Status}")]
    private partial void LogStatusChanged(int itemCount, ItemStatus status);
}