using Microsoft.Extensions.Caching.Hybrid;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Areas;

public sealed record GetFloorPlanQuery : IQuery<IReadOnlyList<AreaDto>>;

internal sealed class GetFloorPlanQueryHandler(IAreaRepository areaRepository, HybridCache cache)
    : IQueryHandler<GetFloorPlanQuery, IReadOnlyList<AreaDto>>
{
    public async Task<Result<IReadOnlyList<AreaDto>>> Handle(
        GetFloorPlanQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<AreaDto> floorPlan = await cache.GetOrCreateAsync(
            CacheKeys.For(query),
            query,
            GetFloorPlanAsync,
            tags: [CacheTags.Tables],
            cancellationToken: cancellationToken);

        return Result.Success(floorPlan);
    }

    // Every area in tab order with its tables and fixtures; a bar has few, so it is one response
    private async ValueTask<IReadOnlyList<AreaDto>> GetFloorPlanAsync(
        GetFloorPlanQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Area> areas = await areaRepository.GetAllAsync(cancellationToken);

        return areas.Select(area => area.ToAreaDto()).ToList();
    }
}