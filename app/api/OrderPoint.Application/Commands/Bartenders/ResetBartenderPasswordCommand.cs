using Microsoft.Extensions.Logging;
using OrderPoint.Application.Identity;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Bartenders;

public sealed record ResetBartenderPasswordCommand(Guid Id, string Password) : ICommand;

internal sealed partial class ResetBartenderPasswordCommandHandler(
    IBartenderRepository bartenderRepository,
    IIdentityService identityService,
    ITokenService tokenService,
    IUnitOfWork unitOfWork,
    ILogger<ResetBartenderPasswordCommandHandler> logger)
    : ICommandHandler<ResetBartenderPasswordCommand>
{
    public async Task<Result> Handle(ResetBartenderPasswordCommand command, CancellationToken cancellationToken)
    {
        Bartender? bartender = await bartenderRepository.GetAsync(command.Id, cancellationToken);

        if (bartender is null)
        {
            return Result.Failure(BartenderErrors.NotFound);
        }

        Result result = await identityService.ResetPasswordAsync(bartender.Id, command.Password, cancellationToken);

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        // Signed out everywhere, so only someone who knows the new temporary password can get back in
        await tokenService.RevokeUserTokensAsync(bartender.Id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        LogPasswordReset(bartender.Id, bartender.Email);

        return Result.Success();
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Password of bartender {BartenderId} ({Email}) reset to a temporary one; sessions signed out")]
    private partial void LogPasswordReset(Guid bartenderId, string email);
}