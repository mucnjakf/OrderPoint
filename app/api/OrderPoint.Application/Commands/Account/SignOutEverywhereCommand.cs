using Microsoft.Extensions.Logging;
using OrderPoint.Application.Identity;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Account;

public sealed record SignOutEverywhereCommand(Guid UserId) : ICommand;

internal sealed partial class SignOutEverywhereCommandHandler(
    ITokenService tokenService,
    IUnitOfWork unitOfWork,
    ILogger<SignOutEverywhereCommandHandler> logger)
    : ICommandHandler<SignOutEverywhereCommand>
{
    public async Task<Result> Handle(SignOutEverywhereCommand command, CancellationToken cancellationToken)
    {
        await tokenService.RevokeUserTokensAsync(command.UserId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        LogSignedOutEverywhere(command.UserId);

        return Result.Success();
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "User {UserId} signed out everywhere")]
    private partial void LogSignedOutEverywhere(Guid userId);
}