using OrderPoint.Application.Identity;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Account;

public sealed record SignOutEverywhereCommand(Guid UserId) : ICommand;

internal sealed class SignOutEverywhereCommandHandler(ITokenService tokenService, IUnitOfWork unitOfWork)
    : ICommandHandler<SignOutEverywhereCommand>
{
    public async Task<Result> Handle(SignOutEverywhereCommand command, CancellationToken cancellationToken)
    {
        await tokenService.RevokeUserTokensAsync(command.UserId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}