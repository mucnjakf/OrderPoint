using Microsoft.Extensions.Caching.Hybrid;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Fixtures;

public sealed record GetFixtureQuery(Guid Id) : IQuery<FixtureDto>;

internal sealed class GetFixtureQueryHandler(IFixtureRepository fixtureRepository, HybridCache cache)
    : IQueryHandler<GetFixtureQuery, FixtureDto>
{
    public async Task<Result<FixtureDto>> Handle(GetFixtureQuery query, CancellationToken cancellationToken)
    {
        FixtureDto? fixtureDto = await cache.GetOrCreateAsync(
            CacheKeys.For(query),
            query,
            GetFixtureAsync,
            tags: [CacheTags.Tables],
            cancellationToken: cancellationToken);

        if (fixtureDto is null)
        {
            return Result.Failure<FixtureDto>(FixtureErrors.NotFound);
        }

        return Result.Success(fixtureDto);
    }

    private async ValueTask<FixtureDto?> GetFixtureAsync(GetFixtureQuery query, CancellationToken cancellationToken)
    {
        Fixture? fixture = await fixtureRepository.GetAsync(query.Id, cancellationToken);

        return fixture?.ToFixtureDto();
    }
}