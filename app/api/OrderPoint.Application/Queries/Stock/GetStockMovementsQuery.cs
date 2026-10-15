using Microsoft.Extensions.Caching.Hybrid;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Stock;

// A stock item's history, the latest first
public sealed record GetStockMovementsQuery(Guid StockItemId, int PageNumber, int PageSize)
    : IQuery<PaginationDto<StockMovementDto>>;

internal sealed class GetStockMovementsQueryHandler(
    IStockItemRepository stockItemRepository,
    IStockMovementRepository stockMovementRepository,
    HybridCache cache)
    : IQueryHandler<GetStockMovementsQuery, PaginationDto<StockMovementDto>>
{
    public async Task<Result<PaginationDto<StockMovementDto>>> Handle(
        GetStockMovementsQuery query,
        CancellationToken cancellationToken)
    {
        PaginationDto<StockMovementDto>? movements = await cache.GetOrCreateAsync(
            CacheKeys.For(query),
            query,
            GetStockMovementsAsync,
            tags: [CacheTags.Stock],
            cancellationToken: cancellationToken);

        if (movements is null)
        {
            return Result.Failure<PaginationDto<StockMovementDto>>(StockItemErrors.NotFound);
        }

        return Result.Success(movements);
    }

    private async ValueTask<PaginationDto<StockMovementDto>?> GetStockMovementsAsync(
        GetStockMovementsQuery query,
        CancellationToken cancellationToken)
    {
        bool stockItemExists = await stockItemRepository.ExistsAsync(query.StockItemId, cancellationToken);

        if (!stockItemExists)
        {
            return null;
        }

        (IReadOnlyList<StockMovement> movements, int totalCount) = await stockMovementRepository
            .GetPaginatedByStockItemAsync(query.StockItemId, query.PageNumber, query.PageSize, cancellationToken);

        return new PaginationDto<StockMovementDto>(
            movements.Select(movement => movement.ToStockMovementDto()).ToList(),
            query.PageNumber,
            query.PageSize,
            totalCount);
    }
}