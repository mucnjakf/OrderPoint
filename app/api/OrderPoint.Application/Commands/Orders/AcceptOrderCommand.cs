using Microsoft.Extensions.Logging;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Orders;

public sealed record AcceptOrderCommand(Guid Id, Guid BartenderId) : ICommand, ICacheInvalidatingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.OrderChanges;
}

internal sealed partial class AcceptOrderCommandHandler(
    IOrderRepository orderRepository,
    IBartenderRepository bartenderRepository,
    IUnitOfWork unitOfWork,
    ILogger<AcceptOrderCommandHandler> logger)
    : ICommandHandler<AcceptOrderCommand>
{
    public async Task<Result> Handle(AcceptOrderCommand command, CancellationToken cancellationToken)
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

        Result result = order.Accept(command.BartenderId);

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        LogOrderAccepted(order.Number, command.BartenderId);

        return Result.Success();
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Order #{OrderNumber} accepted by bartender {BartenderId}")]
    private partial void LogOrderAccepted(int orderNumber, Guid bartenderId);
}