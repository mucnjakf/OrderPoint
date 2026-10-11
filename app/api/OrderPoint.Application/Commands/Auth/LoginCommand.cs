using Microsoft.Extensions.Logging;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Identity;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Application.Telemetry;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Auth;

public sealed record LoginCommand(string Email, string Password, UserRole Role) : ICommand<AuthTokensDto>;

internal sealed partial class LoginCommandHandler(
    IIdentityService identityService,
    ITokenService tokenService,
    IBartenderRepository bartenderRepository,
    IUnitOfWork unitOfWork,
    ApplicationTelemetry telemetry,
    ILogger<LoginCommandHandler> logger)
    : ICommandHandler<LoginCommand, AuthTokensDto>
{
    public async Task<Result<AuthTokensDto>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        Result<AuthTokensDto> result = await LoginAsync(command, cancellationToken);

        telemetry.RecordLogin(command.Role, result);

        if (result.IsSuccess)
        {
            LogSignedIn(command.Email, command.Role);
        }
        else
        {
            LogLoginFailed(command.Email, command.Role, result.Error.Code);
        }

        return result;
    }

    private async Task<Result<AuthTokensDto>> LoginAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        Result<UserDto> userResult = await identityService.CheckPasswordAsync(
            command.Email,
            command.Password,
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

        if (user.MustChangePassword)
        {
            return Result.Failure<AuthTokensDto>(AuthErrors.PasswordChangeRequired);
        }

        AuthTokensDto tokens = await tokenService.CreateTokensAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(tokens);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "{Email} signed in as {Role}")]
    private partial void LogSignedIn(string email, UserRole role);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Login failed for {Email} as {Role}: {ErrorCode}")]
    private partial void LogLoginFailed(string email, UserRole role, string errorCode);
}