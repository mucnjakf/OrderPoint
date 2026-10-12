using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Mediator;

internal sealed class CacheInvalidationBehavior<TRequest, TResponse>(HybridCache cache)
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

        // Not the request's token: once the change is saved, stale reads must go even if the caller has left
        if (response.IsSuccess && request is ICacheInvalidatingCommand command)
        {
            await cache.RemoveByTagAsync(command.InvalidatedCacheTags, CancellationToken.None);
        }

        return response;
    }
}