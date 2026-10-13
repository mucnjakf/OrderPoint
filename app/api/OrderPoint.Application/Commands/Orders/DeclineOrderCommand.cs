using Microsoft.Extensions.Logging;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Orders;

public sealed record DeclineOrderCommand(Guid Id, Guid BartenderId)
    : ICommand, ICacheInvalidatingCommand, IOrderNotifyingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.OrderChanges;
}

internal sealed partial class DeclineOrderCommandHandler(
    IOrderRepository orderRepository,
    IBartenderRepository bartenderRepository,
    IUnitOfWork unitOfWork,
    ILogger<DeclineOrderCommandHandler> logger)
    : ICommandHandler<DeclineOrderCommand>
{
    public async Task<Result> Handle(DeclineOrderCommand command, CancellationToken cancellationToken)
    {
        Order? order = await orderRepository.GetAsync(command.Id, cancellationToken);

        if (order is null)
        {
            return Result.Failure(OrderErrors.NotFound);
        }

        bool bartenderExists = await bartenderRepository.ExistsAsync(command.BartenderId, cancellationToken);

        if (!bartenderExists)
        {
            return Result.Failure(BartenderErrors.NotFound);
        }

        Result result = order.Decline(command.BartenderId);

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        LogOrderDeclined(order.Number, command.BartenderId);

        return Result.Success();
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Order #{OrderNumber} declined by bartender {BartenderId}")]
    private partial void LogOrderDeclined(int orderNumber, Guid bartenderId);
}