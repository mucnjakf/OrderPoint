using OrderPoint.Application.Dtos;
using OrderPoint.Application.Identity;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Application.Storage;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Account;

public sealed record UpdateAccountImageCommand(Guid UserId, Stream Content, string ContentType) : ICommand;

internal sealed class UpdateAccountImageCommandHandler(
    IIdentityService identityService,
    IImageStorage imageStorage,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateAccountImageCommand>
{
    private const string ImageFolder = "users";

    public async Task<Result> Handle(UpdateAccountImageCommand command, CancellationToken cancellationToken)
    {
        UserDto? user = await identityService.GetUserAsync(command.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(AuthErrors.UserNotFound);
        }

        string imageUrl = await imageStorage.UploadAsync(
            command.Content,
            command.ContentType,
            ImageFolder,
            cancellationToken);

        Result result = await identityService.SetImageUrlAsync(user.Id, imageUrl, cancellationToken);

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (user.ImageUrl is not null)
        {
            await imageStorage.DeleteAsync(user.ImageUrl, cancellationToken);
        }

        return Result.Success();
    }
}