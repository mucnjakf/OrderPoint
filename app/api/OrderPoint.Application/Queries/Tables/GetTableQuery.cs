using Microsoft.Extensions.Caching.Hybrid;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Tables;

public sealed record GetTableQuery(Guid Id) : IQuery<TableDto>;

internal sealed class GetTableQueryHandler(ITableRepository tableRepository, HybridCache cache)
    : IQueryHandler<GetTableQuery, TableDto>
{
    public async Task<Result<TableDto>> Handle(GetTableQuery query, CancellationToken cancellationToken)
    {
        TableDto? tableDto = await cache.GetOrCreateAsync(
            CacheKeys.For(query),
            query,
            GetTableAsync,
            tags: [CacheTags.Tables],
            cancellationToken: cancellationToken);

        if (tableDto is null)
        {
            return Result.Failure<TableDto>(TableErrors.NotFound);
        }

        return Result.Success(tableDto);
    }

    private async ValueTask<TableDto?> GetTableAsync(GetTableQuery query, CancellationToken cancellationToken)
    {
        Table? table = await tableRepository.GetAsync(query.Id, cancellationToken);

        return table?.ToTableDto();
    }
}