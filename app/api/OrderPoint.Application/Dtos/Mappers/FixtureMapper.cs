using OrderPoint.Domain.Entities;

namespace OrderPoint.Application.Dtos.Mappers;

internal static class FixtureMapper
{
    internal static FixtureDto ToFixtureDto(this Fixture fixture) => new(
        fixture.Id,
        fixture.AreaId,
        fixture.Type,
        fixture.Label,
        fixture.X,
        fixture.Y,
        fixture.Width,
        fixture.Height,
        fixture.CreatedAtUtc,
        fixture.UpdatedAtUtc);
}