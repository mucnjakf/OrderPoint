using Microsoft.Extensions.Logging;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Identity;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Notifications;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Bartenders;

public sealed record ChangeBartendersStatusCommand(IReadOnlyList<Guid> Ids, BartenderStatus Status)
    : ICommand, ICacheInvalidatingCommand, IOrderNotifyingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.BartenderChanges;
}

internal sealed partial class ChangeBartendersStatusCommandHandler(
    IBartenderRepository bartenderRepository,
    IOrderRepository orderRepository,
    ITokenService tokenService,
    IUnitOfWork unitOfWork,
    ChangedOrders changedOrders,
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

        List<Guid> deactivatedIds = [];

        foreach (Bartender bartender in bartenders)
        {
            bool isDeactivated =
                bartender.Status == BartenderStatus.Active && command.Status == BartenderStatus.Inactive;

            bartender.ChangeStatus(command.Status);

            if (isDeactivated)
            {
                await tokenService.RevokeUserTokensAsync(bartender.Id, cancellationToken);
                deactivatedIds.Add(bartender.Id);
            }
        }

        // Their accepted and active orders go back to the queue, so they aren't stuck with nobody to finish them
        IReadOnlyList<Order> openOrders = deactivatedIds.Count == 0
            ? []
            : await orderRepository.GetOpenByBartendersAsync(deactivatedIds, cancellationToken);

        foreach (Order order in openOrders)
        {
            order.Release();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (Order order in openOrders)
        {
            changedOrders.Add(order.Id);
        }

        LogStatusChanged(bartenders.Count, command.Status, deactivatedIds.Count, openOrders.Count);

        return Result.Success();
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "{BartenderCount} bartenders set to {Status}; {SignedOutCount} deactivated and signed out, " +
                  "{ReleasedOrderCount} of their open orders back in the queue")]
    private partial void LogStatusChanged(
        int bartenderCount,
        BartenderStatus status,
        int signedOutCount,
        int releasedOrderCount);
}