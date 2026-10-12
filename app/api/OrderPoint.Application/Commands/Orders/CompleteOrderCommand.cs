using Microsoft.Extensions.Logging;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Orders;

public sealed record CompleteOrderCommand(Guid Id, Guid BartenderId) : ICommand, ICacheInvalidatingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.OrderChanges;
}

internal sealed partial class CompleteOrderCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork,
    ILogger<CompleteOrderCommandHandler> logger)
    : ICommandHandler<CompleteOrderCommand>
{
    public async Task<Result> Handle(CompleteOrderCommand command, CancellationToken cancellationToken)
    {
        Order? order = await orderRepository.GetAsync(command.Id, cancellationToken);

        if (order is null)
        {
            return Result.Failure(OrderErrors.NotFound);
        }

        Result result = order.Complete(command.BartenderId);

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        LogOrderCompleted(order.Number, command.BartenderId);

        return Result.Success();
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Order #{OrderNumber} completed by bartender {BartenderId}")]
    private partial void LogOrderCompleted(int orderNumber, Guid bartenderId);
}