using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Items;

public sealed record DeleteItemCommand(Guid Id) : ICommand;

internal sealed class DeleteItemCommandHandler(IItemRepository itemRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteItemCommand>
{
    public async Task<Result> Handle(DeleteItemCommand command, CancellationToken cancellationToken)
    {
        Item? item = await itemRepository.GetAsync(command.Id, cancellationToken);

        if (item is null)
        {
            return Result.Failure(ItemErrors.NotFound);
        }

        itemRepository.Delete(item);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}