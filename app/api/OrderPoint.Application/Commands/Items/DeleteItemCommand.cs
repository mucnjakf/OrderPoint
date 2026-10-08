using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Application.Storage;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Items;

public sealed record DeleteItemCommand(Guid Id) : ICommand;

internal sealed class DeleteItemCommandHandler(
    IItemRepository itemRepository,
    IOrderRepository orderRepository,
    IImageStorage imageStorage,
    IUnitOfWork unitOfWork)
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

        itemRepository.Delete(item);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (item.ImageUrl is not null)
        {
            await imageStorage.DeleteAsync(item.ImageUrl, cancellationToken);
        }

        return Result.Success();
    }
}