using OrderPoint.Application.Dtos;
using OrderPoint.Application.Identity;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Application.Storage;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Account;

public sealed record DeleteAccountImageCommand(Guid UserId) : ICommand;

internal sealed class DeleteAccountImageCommandHandler(
    IIdentityService identityService,
    IImageStorage imageStorage,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteAccountImageCommand>
{
    public async Task<Result> Handle(DeleteAccountImageCommand command, CancellationToken cancellationToken)
    {
        UserDto? user = await identityService.GetUserAsync(command.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(AuthErrors.UserNotFound);
        }

        if (user.ImageUrl is null)
        {
            return Result.Success();
        }

        Result result = await identityService.SetImageUrlAsync(user.Id, imageUrl: null, cancellationToken);

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await imageStorage.DeleteAsync(user.ImageUrl, cancellationToken);

        return Result.Success();
    }
}