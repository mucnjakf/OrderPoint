using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Application.Storage;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Items;

public sealed record DeleteItemsCommand(IReadOnlyList<Guid> Ids) : ICommand;

internal sealed class DeleteItemsCommandHandler(
    IItemRepository itemRepository,
    IOrderRepository orderRepository,
    IImageStorage imageStorage,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteItemsCommand>
{
    public async Task<Result> Handle(DeleteItemsCommand command, CancellationToken cancellationToken)
    {
        IReadOnlyList<Item> items = await itemRepository.GetAsync(command.Ids, cancellationToken);

        if (items.Count != command.Ids.Count)
        {
            return Result.Failure(ItemErrors.NotFound);
        }

        bool haveOrders = await orderRepository.ExistsByItemsAsync(command.Ids, cancellationToken);

        if (haveOrders)
        {
            return Result.Failure(ItemErrors.CannotDeleteItemWithOrders);
        }

        foreach (Item item in items)
        {
            itemRepository.Delete(item);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (Item item in items.Where(item => item.ImageUrl is not null))
        {
            await imageStorage.DeleteAsync(item.ImageUrl!, cancellationToken);
        }

        return Result.Success();
    }
}