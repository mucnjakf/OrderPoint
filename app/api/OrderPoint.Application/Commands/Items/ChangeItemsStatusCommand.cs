using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Items;

public sealed record ChangeItemsStatusCommand(IReadOnlyList<Guid> Ids, ItemStatus Status) : ICommand;

internal sealed class ChangeItemsStatusCommandHandler(
    IItemRepository itemRepository,
    IUnitOfWork unitOfWork)
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

        return Result.Success();
    }
}