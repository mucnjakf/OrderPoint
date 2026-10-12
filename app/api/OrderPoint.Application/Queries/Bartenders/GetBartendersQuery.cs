using Microsoft.Extensions.Caching.Hybrid;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;
using OrderPoint.Domain.Sorting;

namespace OrderPoint.Application.Queries.Bartenders;

public sealed record GetBartendersQuery(
    int PageNumber,
    int PageSize,
    string? SearchQuery,
    BartenderStatus? Status,
    BartenderSortBy? SortBy)
    : IQuery<PaginationDto<BartenderDto>>;

internal sealed class GetBartendersQueryHandler(
    IBartenderRepository bartenderRepository,
    IOrderRepository orderRepository,
    HybridCache cache)
    : IQueryHandler<GetBartendersQuery, PaginationDto<BartenderDto>>
{
    public async Task<Result<PaginationDto<BartenderDto>>> Handle(
        GetBartendersQuery query,
        CancellationToken cancellationToken)
    {
        PaginationDto<BartenderDto> bartenders = await cache.GetOrCreateAsync(
            CacheKeys.For(query),
            query,
            GetBartendersAsync,
            tags: [CacheTags.Bartenders],
            cancellationToken: cancellationToken);

        return Result.Success(bartenders);
    }

    private async ValueTask<PaginationDto<BartenderDto>> GetBartendersAsync(
        GetBartendersQuery query,
        CancellationToken cancellationToken)
    {
        (IReadOnlyList<Bartender> bartenders, int totalCount) = await bartenderRepository
            .GetPaginatedAsync(
                query.PageNumber,
                query.PageSize,
                query.SearchQuery,
                query.Status,
                query.SortBy,
                cancellationToken);

        IReadOnlyDictionary<Guid, int> ordersCounts = await orderRepository.CountByBartendersAsync(
            bartenders.Select(bartender => bartender.Id).ToList(),
            cancellationToken);

        return new PaginationDto<BartenderDto>(
            bartenders.Select(bartender => bartender.ToBartenderDto(ordersCounts.GetValueOrDefault(bartender.Id)))
                .ToList(),
            query.PageNumber,
            query.PageSize,
            totalCount);
    }
}