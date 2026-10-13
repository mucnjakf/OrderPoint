using Microsoft.Extensions.Logging;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Orders;

public sealed record AcceptOrderCommand(Guid Id, Guid BartenderId)
    : ICommand, ICacheInvalidatingCommand, IOrderNotifyingCommand
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

        Bartender? bartender = await bartenderRepository.GetAsync(command.BartenderId, cancellationToken);

        if (bartender is null)
        {
            return Result.Failure(BartenderErrors.NotFound);
        }

        // The access token outlives a deactivation by up to 15 minutes
        if (bartender.Status == BartenderStatus.Inactive)
        {
            return Result.Failure(AuthErrors.AccountInactive);
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