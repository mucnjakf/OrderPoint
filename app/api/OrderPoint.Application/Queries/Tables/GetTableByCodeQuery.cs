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

// The customer app checks the code from a QR code before it remembers the table
public sealed record GetTableByCodeQuery(string Code) : IQuery<GuestTableDto>;

internal sealed class GetTableByCodeQueryHandler(ITableRepository tableRepository, HybridCache cache)
    : IQueryHandler<GetTableByCodeQuery, GuestTableDto>
{
    public async Task<Result<GuestTableDto>> Handle(GetTableByCodeQuery query, CancellationToken cancellationToken)
    {
        GuestTableDto? tableDto = await cache.GetOrCreateAsync(
            CacheKeys.For(query),
            query,
            GetTableAsync,
            tags: [CacheTags.Tables],
            cancellationToken: cancellationToken);

        if (tableDto is null)
        {
            return Result.Failure<GuestTableDto>(TableErrors.NotFound);
        }

        return Result.Success(tableDto);
    }

    private async ValueTask<GuestTableDto?> GetTableAsync(
        GetTableByCodeQuery query,
        CancellationToken cancellationToken)
    {
        Table? table = await tableRepository.GetByCodeAsync(query.Code, cancellationToken);

        return table?.ToGuestTableDto();
    }
}