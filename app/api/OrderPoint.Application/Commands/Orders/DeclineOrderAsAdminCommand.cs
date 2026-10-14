using Microsoft.Extensions.Logging;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Notifications;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Orders;

// The admin closes an open order nobody will finish, e.g. at closing time
public sealed record DeclineOrderAsAdminCommand(Guid Id)
    : ICommand, ICacheInvalidatingCommand, IOrderNotifyingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.OrderChanges;
}

internal sealed partial class DeclineOrderAsAdminCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork,
    ChangedOrders changedOrders,
    ILogger<DeclineOrderAsAdminCommandHandler> logger)
    : ICommandHandler<DeclineOrderAsAdminCommand>
{
    public async Task<Result> Handle(DeclineOrderAsAdminCommand command, CancellationToken cancellationToken)
    {
        Order? order = await orderRepository.GetAsync(command.Id, cancellationToken);

        if (order is null)
        {
            return Result.Failure(OrderErrors.NotFound);
        }

        Result result = order.DeclineByAdmin();

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        changedOrders.Add(order.Id);

        LogOrderDeclined(order.Number);

        return Result.Success();
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Order #{OrderNumber} declined by the admin")]
    private partial void LogOrderDeclined(int orderNumber);
}