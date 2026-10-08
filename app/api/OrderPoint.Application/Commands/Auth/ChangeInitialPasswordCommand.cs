using OrderPoint.Application.Dtos;
using OrderPoint.Application.Identity;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Auth;

public sealed record ChangeInitialPasswordCommand(
    string Email,
    string CurrentPassword,
    string NewPassword,
    UserRole Role)
    : ICommand<AuthTokensDto>;

internal sealed class ChangeInitialPasswordCommandHandler(
    IIdentityService identityService,
    ITokenService tokenService,
    IBartenderRepository bartenderRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ChangeInitialPasswordCommand, AuthTokensDto>
{
    public async Task<Result<AuthTokensDto>> Handle(
        ChangeInitialPasswordCommand command,
        CancellationToken cancellationToken)
    {
        Result<UserDto> userResult = await identityService.CheckPasswordAsync(
            command.Email,
            command.CurrentPassword,
            command.Role,
            cancellationToken);

        // Failed attempts and lockouts are tracked on the user, so they are saved even when the check fails
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (userResult.IsFailure)
        {
            return Result.Failure<AuthTokensDto>(userResult.Error);
        }

        UserDto user = userResult.Value;

        if (user.Role == UserRole.Bartender)
        {
            Bartender? bartender = await bartenderRepository.GetAsync(user.Id, cancellationToken);

            if (bartender is null || bartender.Status == BartenderStatus.Inactive)
            {
                return Result.Failure<AuthTokensDto>(AuthErrors.AccountInactive);
            }
        }

        if (!user.MustChangePassword)
        {
            return Result.Failure<AuthTokensDto>(AuthErrors.PasswordChangeNotAllowed);
        }

        Result result = await identityService.ChangeInitialPasswordAsync(
            user.Id,
            command.CurrentPassword,
            command.NewPassword,
            cancellationToken);

        if (result.IsFailure)
        {
            return Result.Failure<AuthTokensDto>(result.Error);
        }

        AuthTokensDto tokens = await tokenService.CreateTokensAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(tokens);
    }
}