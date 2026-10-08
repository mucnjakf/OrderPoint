using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Application.Storage;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Bartenders;

public sealed record DeleteBartenderImageCommand(Guid Id) : ICommand;

internal sealed class DeleteBartenderImageCommandHandler(
    IBartenderRepository bartenderRepository,
    IImageStorage imageStorage,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteBartenderImageCommand>
{
    public async Task<Result> Handle(DeleteBartenderImageCommand command, CancellationToken cancellationToken)
    {
        Bartender? bartender = await bartenderRepository.GetAsync(command.Id, cancellationToken);

        if (bartender is null)
        {
            return Result.Failure(BartenderErrors.NotFound);
        }

        if (bartender.ImageUrl is null)
        {
            return Result.Success();
        }

        string imageUrl = bartender.ImageUrl;

        bartender.RemoveImage();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await imageStorage.DeleteAsync(imageUrl, cancellationToken);

        return Result.Success();
    }
}