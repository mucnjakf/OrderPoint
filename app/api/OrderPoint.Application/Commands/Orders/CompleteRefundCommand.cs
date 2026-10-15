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

// The register handed back the money of a declined or cancelled order it had taken
public sealed record CompleteRefundCommand(Guid Id, Guid BartenderId)
    : ICommand, ICacheInvalidatingCommand, IOrderNotifyingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.OrderChanges;
}

internal sealed partial class CompleteRefundCommandHandler(
    IOrderRepository orderRepository,
    IBartenderRepository bartenderRepository,
    IUnitOfWork unitOfWork,
    ChangedOrders changedOrders,
    ILogger<CompleteRefundCommandHandler> logger)
    : ICommandHandler<CompleteRefundCommand>
{
    public async Task<Result> Handle(CompleteRefundCommand command, CancellationToken cancellationToken)
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

        Result result = order.CompleteRefund();

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        changedOrders.Add(order.Id);

        LogRefundCompleted(order.Number, command.BartenderId, order.Total);

        return Result.Success();
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Order #{OrderNumber} refunded at the register by bartender {BartenderId}: {Total} EUR")]
    private partial void LogRefundCompleted(int orderNumber, Guid bartenderId, decimal total);
}