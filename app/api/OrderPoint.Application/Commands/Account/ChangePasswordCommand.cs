using OrderPoint.Application.Dtos;
using OrderPoint.Application.Identity;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Account;

public sealed record ChangePasswordCommand(Guid UserId, string CurrentPassword, string NewPassword)
    : ICommand<AuthTokensDto>;

internal sealed class ChangePasswordCommandHandler(
    IIdentityService identityService,
    ITokenService tokenService,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ChangePasswordCommand, AuthTokensDto>
{
    public async Task<Result<AuthTokensDto>> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        UserDto? user = await identityService.GetUserAsync(command.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<AuthTokensDto>(AuthErrors.UserNotFound);
        }

        Result result = await identityService.ChangePasswordAsync(
            user.Id,
            command.CurrentPassword,
            command.NewPassword,
            cancellationToken);

        if (result.IsFailure)
        {
            return Result.Failure<AuthTokensDto>(result.Error);
        }

        // Every other session is signed out; this one continues with a fresh token pair
        await tokenService.RevokeUserTokensAsync(user.Id, cancellationToken);
        AuthTokensDto tokens = await tokenService.CreateTokensAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(tokens);
    }
}