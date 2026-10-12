using Microsoft.Extensions.Logging;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Identity;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Bartenders;

public sealed record ChangeBartendersStatusCommand(IReadOnlyList<Guid> Ids, BartenderStatus Status)
    : ICommand, ICacheInvalidatingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.BartenderChanges;
}

internal sealed partial class ChangeBartendersStatusCommandHandler(
    IBartenderRepository bartenderRepository,
    ITokenService tokenService,
    IUnitOfWork unitOfWork,
    ILogger<ChangeBartendersStatusCommandHandler> logger)
    : ICommandHandler<ChangeBartendersStatusCommand>
{
    public async Task<Result> Handle(ChangeBartendersStatusCommand command, CancellationToken cancellationToken)
    {
        IReadOnlyList<Bartender> bartenders = await bartenderRepository.GetAsync(command.Ids, cancellationToken);

        if (bartenders.Count != command.Ids.Count)
        {
            return Result.Failure(BartenderErrors.NotFound);
        }

        int deactivatedCount = 0;

        foreach (Bartender bartender in bartenders)
        {
            bool isDeactivated =
                bartender.Status == BartenderStatus.Active && command.Status == BartenderStatus.Inactive;

            bartender.ChangeStatus(command.Status);

            if (isDeactivated)
            {
                await tokenService.RevokeUserTokensAsync(bartender.Id, cancellationToken);
                deactivatedCount++;
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        LogStatusChanged(bartenders.Count, command.Status, deactivatedCount);

        return Result.Success();
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "{BartenderCount} bartenders set to {Status}; {SignedOutCount} deactivated and signed out")]
    private partial void LogStatusChanged(int bartenderCount, BartenderStatus status, int signedOutCount);
}