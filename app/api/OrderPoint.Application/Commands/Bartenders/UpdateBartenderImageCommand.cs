using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Application.Storage;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Bartenders;

public sealed record UpdateBartenderImageCommand(Guid Id, Stream Content, string ContentType) : ICommand;

internal sealed class UpdateBartenderImageCommandHandler(
    IBartenderRepository bartenderRepository,
    IImageStorage imageStorage,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateBartenderImageCommand>
{
    private const string ImageFolder = "bartenders";

    public async Task<Result> Handle(UpdateBartenderImageCommand command, CancellationToken cancellationToken)
    {
        Bartender? bartender = await bartenderRepository.GetAsync(command.Id, cancellationToken);

        if (bartender is null)
        {
            return Result.Failure(BartenderErrors.NotFound);
        }

        string? previousImageUrl = bartender.ImageUrl;

        string imageUrl = await imageStorage.UploadAsync(
            command.Content,
            command.ContentType,
            ImageFolder,
            cancellationToken);

        Result result = bartender.SetImage(imageUrl);

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