using Microsoft.Extensions.Logging;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Notifications;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Orders;

public sealed record WithdrawOrderCommand(Guid Id, Guid BartenderId)
    : ICommand, ICacheInvalidatingCommand, IOrderNotifyingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.OrderChanges;
}

internal sealed partial class WithdrawOrderCommandHandler(
    IOrderRepository orderRepository,
    IBartenderRepository bartenderRepository,
    IUnitOfWork unitOfWork,
    ChangedOrders changedOrders,
    ILogger<WithdrawOrderCommandHandler> logger)
    : ICommandHandler<WithdrawOrderCommand>
{
    public async Task<Result> Handle(WithdrawOrderCommand command, CancellationToken cancellationToken)
    {
        Order? order = await orderRepository.GetAsync(command.Id, cancellationToken);

        if (order is null)
        {
            return Result.Failure(OrderErrors.NotFound);
        }

        Bartender? bartender = await bartenderRepository.GetAsync(command.BartenderId, cancellationToken);

        // The access token outlives a deletion by up to 15 minutes
        if (bartender is null)
        {
            return Result.Failure(AuthErrors.AccountDeleted);
        }

        // The access token outlives a deactivation by up to 15 minutes
        if (bartender.Status == BartenderStatus.Inactive)
        {
            return Result.Failure(AuthErrors.AccountInactive);
        }

        Result result = order.Withdraw(command.BartenderId);

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        changedOrders.Add(order.Id);

        LogOrderWithdrawn(order.Number, command.BartenderId);

        return Result.Success();
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Order #{OrderNumber} withdrawn by bartender {BartenderId}")]
    private partial void LogOrderWithdrawn(int orderNumber, Guid bartenderId);
}