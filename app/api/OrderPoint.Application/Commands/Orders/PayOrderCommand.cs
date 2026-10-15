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

// An order taken in the Bartender app, paid at the register
public sealed record PayOrderCommand(Guid Id, PaymentMethod PaymentMethod, decimal? CashReceived, Guid BartenderId)
    : ICommand, ICacheInvalidatingCommand, IOrderNotifyingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.OrderChanges;
}

internal sealed partial class PayOrderCommandHandler(
    IOrderRepository orderRepository,
    IBartenderRepository bartenderRepository,
    IUnitOfWork unitOfWork,
    ChangedOrders changedOrders,
    ILogger<PayOrderCommandHandler> logger)
    : ICommandHandler<PayOrderCommand>
{
    public async Task<Result> Handle(PayOrderCommand command, CancellationToken cancellationToken)
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

        Result result = order.PayAtRegister(command.PaymentMethod, command.CashReceived, command.BartenderId);

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        changedOrders.Add(order.Id);

        LogOrderPaid(order.Number, command.BartenderId, command.PaymentMethod, order.Total);

        return Result.Success();
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Order #{OrderNumber} paid at the register to bartender {BartenderId}: {PaymentMethod}, {Total} EUR")]
    private partial void LogOrderPaid(int orderNumber, Guid bartenderId, PaymentMethod paymentMethod, decimal total);
}