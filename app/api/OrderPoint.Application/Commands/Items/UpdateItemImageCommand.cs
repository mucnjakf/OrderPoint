using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Application.Storage;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Items;

public sealed record UpdateItemImageCommand(Guid Id, Stream Content, string ContentType) : ICommand;

internal sealed class UpdateItemImageCommandHandler(
    IItemRepository itemRepository,
    IImageStorage imageStorage,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateItemImageCommand>
{
    private const string ImageFolder = "items";

    public async Task<Result> Handle(UpdateItemImageCommand command, CancellationToken cancellationToken)
    {
        Item? item = await itemRepository.GetAsync(command.Id, cancellationToken);

        if (item is null)
        {
            return Result.Failure(ItemErrors.NotFound);
        }

        string? previousImageUrl = item.ImageUrl;

        string imageUrl = await imageStorage.UploadAsync(
            command.Content,
            command.ContentType,
            ImageFolder,
            cancellationToken);

        Result result = item.SetImage(imageUrl);

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (previousImageUrl is not null)
        {
            await imageStorage.DeleteAsync(previousImageUrl, cancellationToken);
        }

        return Result.Success();
    }
}