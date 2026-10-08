using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Application.Storage;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Items;

public sealed record DeleteItemImageCommand(Guid Id) : ICommand;

internal sealed class DeleteItemImageCommandHandler(
    IItemRepository itemRepository,
    IImageStorage imageStorage,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteItemImageCommand>
{
    public async Task<Result> Handle(DeleteItemImageCommand command, CancellationToken cancellationToken)
    {
        Item? item = await itemRepository.GetAsync(command.Id, cancellationToken);

        if (item is null)
        {
            return Result.Failure(ItemErrors.NotFound);
        }

        if (item.ImageUrl is null)
        {
            return Result.Success();
        }

        string imageUrl = item.ImageUrl;

        item.RemoveImage();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await imageStorage.DeleteAsync(imageUrl, cancellationToken);

        return Result.Success();
    }
}