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

public sealed record UpdateBartenderCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    BartenderStatus Status,
    string? Notes) : ICommand, ICacheInvalidatingCommand, IOrderNotifyingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.BartenderChanges;
}

internal sealed partial class UpdateBartenderCommandHandler(
    IBartenderRepository bartenderRepository,
    IOrderRepository orderRepository,
    IIdentityService identityService,
    ITokenService tokenService,
    IUnitOfWork unitOfWork,
    ChangedOrders changedOrders,
    ILogger<UpdateBartenderCommandHandler> logger)
    : ICommandHandler<UpdateBartenderCommand>
{
    public async Task<Result> Handle(UpdateBartenderCommand command, CancellationToken cancellationToken)
    {
        Bartender? bartender = await bartenderRepository.GetAsync(command.Id, cancellationToken);

        if (bartender is null)
        {
            return Result.Failure(BartenderErrors.NotFound);
        }

        bool isEmailChanged = command.Email != bartender.Email;
        bool isDeactivated = bartender.Status == BartenderStatus.Active && command.Status == BartenderStatus.Inactive;

        if (isEmailChanged)
        {
            bool emailExists = await bartenderRepository.ExistsByEmailAsync(command.Email, cancellationToken);

            if (emailExists)
            {
                return Result.Failure(BartenderErrors.EmailAlreadyExists);
            }
        }

        Result result = bartender.Update(
            command.FirstName,
            command.LastName,
            command.Email,
            command.PhoneNumber,
            command.Status,
            command.Notes);

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        if (isEmailChanged)
        {
            Result userResult = await identityService.UpdateEmailAsync(bartender.Id, command.Email, cancellationToken);

            if (userResult.IsFailure)
            {
                return Result.Failure(userResult.Error);
            }
        }

        IReadOnlyList<Order> openOrders = [];

        if (isDeactivated)
        {
            await tokenService.RevokeUserTokensAsync(bartender.Id, cancellationToken);

            // Their accepted and active orders go back to the queue, so they aren't stuck with nobody to finish them
            openOrders = await orderRepository.GetOpenByBartendersAsync([bartender.Id], cancellationToken);

            foreach (Order order in openOrders)
            {
                order.Release();
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (Order order in openOrders)
        {
            changedOrders.Add(order.Id);
        }

        if (openOrders.Count != 0)
        {
            LogOrdersReleased(openOrders.Count, bartender.Id);
        }

        return Result.Success();
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "{ReleasedOrderCount} open orders of deactivated bartender {BartenderId} back in the queue")]
    private partial void LogOrdersReleased(int releasedOrderCount, Guid bartenderId);
}