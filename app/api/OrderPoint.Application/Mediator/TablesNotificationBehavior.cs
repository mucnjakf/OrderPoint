using MediatR;
using OrderPoint.Application.Notifications;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Mediator;

// Registered outside CacheInvalidationBehavior, so apps that reload on the signal never read stale cache entries
internal sealed class TablesNotificationBehavior<TRequest, TResponse>(ITablesNotifier tablesNotifier)
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

        if (response.IsSuccess && request is ITablesNotifyingCommand)
        {
            await tablesNotifier.NotifyTablesChangedAsync(CancellationToken.None);
        }

        return response;
    }
}