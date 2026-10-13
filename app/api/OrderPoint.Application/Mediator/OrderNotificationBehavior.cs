using MediatR;
using OrderPoint.Application.Notifications;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Mediator;

// Registered outside CacheInvalidationBehavior, so apps that reload on the signal never read stale cache entries
internal sealed class OrderNotificationBehavior<TRequest, TResponse>(IOrderNotifier orderNotifier)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        TResponse response = await next(cancellationToken);

        if (response.IsSuccess && request is IOrderNotifyingCommand)
        {
            await orderNotifier.NotifyOrdersChangedAsync(CancellationToken.None);
        }

        return response;
    }
}