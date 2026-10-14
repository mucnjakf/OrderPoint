using Microsoft.Extensions.Caching.Hybrid;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Areas;

public sealed record GetAreaQuery(Guid Id) : IQuery<AreaDto>;

internal sealed class GetAreaQueryHandler(IAreaRepository areaRepository, HybridCache cache)
    : IQueryHandler<GetAreaQuery, AreaDto>
{
    public async Task<Result<AreaDto>> Handle(GetAreaQuery query, CancellationToken cancellationToken)
    {
        AreaDto? areaDto = await cache.GetOrCreateAsync(
            CacheKeys.For(query),
            query,
            GetAreaAsync,
            tags: [CacheTags.Tables],
            cancellationToken: cancellationToken);

        if (areaDto is null)
        {
            return Result.Failure<AreaDto>(AreaErrors.NotFound);
        }

        return Result.Success(areaDto);
    }

    private async ValueTask<AreaDto?> GetAreaAsync(GetAreaQuery query, CancellationToken cancellationToken)
    {
        Area? area = await areaRepository.GetAsync(query.Id, cancellationToken);

        return area?.ToAreaDto();
    }
}