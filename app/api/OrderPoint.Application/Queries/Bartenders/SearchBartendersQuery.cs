using Microsoft.Extensions.Caching.Hybrid;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Bartenders;

public sealed record SearchBartendersQuery(string SearchQuery) : IQuery<IReadOnlyList<BartenderDto>>;

internal sealed class SearchBartendersQueryHandler(
    IBartenderRepository bartenderRepository,
    IOrderRepository orderRepository,
    HybridCache cache)
    : IQueryHandler<SearchBartendersQuery, IReadOnlyList<BartenderDto>>
{
    public async Task<Result<IReadOnlyList<BartenderDto>>> Handle(
        SearchBartendersQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<BartenderDto> bartenders = await cache.GetOrCreateAsync(
            CacheKeys.For(query),
            query,
            SearchBartendersAsync,
            tags: [CacheTags.Bartenders],
            cancellationToken: cancellationToken);

        return Result.Success(bartenders);
    }

    private async ValueTask<IReadOnlyList<BartenderDto>> SearchBartendersAsync(
        SearchBartendersQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Bartender> bartenders = await bartenderRepository
            .SearchAsync(query.SearchQuery, cancellationToken);

        IReadOnlyDictionary<Guid, int> ordersCounts = await orderRepository.CountByBartendersAsync(
            bartenders.Select(bartender => bartender.Id).ToList(),
            cancellationToken);

        return bartenders
            .Select(bartender => bartender.ToBartenderDto(ordersCounts.GetValueOrDefault(bartender.Id)))
            .ToList();
    }
}