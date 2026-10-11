using OrderPoint.Application.Identity;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Bartenders;

public sealed record ChangeBartendersStatusCommand(IReadOnlyList<Guid> Ids, BartenderStatus Status) : ICommand;

internal sealed class ChangeBartendersStatusCommandHandler(
    IBartenderRepository bartenderRepository,
    ITokenService tokenService,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ChangeBartendersStatusCommand>
{
    public async Task<Result> Handle(ChangeBartendersStatusCommand command, CancellationToken cancellationToken)
    {
        IReadOnlyList<Bartender> bartenders = await bartenderRepository.GetAsync(command.Ids, cancellationToken);

        if (bartenders.Count != command.Ids.Count)
        {
            return Result.Failure(BartenderErrors.NotFound);
        }

        foreach (Bartender bartender in bartenders)
        {
            bool isDeactivated =
                bartender.Status == BartenderStatus.Active && command.Status == BartenderStatus.Inactive;

            bartender.ChangeStatus(command.Status);

            if (isDeactivated)
            {
                await tokenService.RevokeUserTokensAsync(bartender.Id, cancellationToken);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}