using OrderPoint.Domain.Entities;

namespace OrderPoint.Application.Dtos.Mappers;

internal static class AreaMapper
{
    internal static AreaDto ToAreaDto(this Area area) => new(
        area.Id,
        area.Name,
        area.Columns,
        area.Rows,
        area.DisplayOrder,
        area.Tables
            .OrderBy(table => table.Code)
            .Select(table => table.ToTableDto())
            .ToList(),
        area.Fixtures
            .OrderBy(fixture => fixture.CreatedAtUtc)
            .Select(fixture => fixture.ToFixtureDto())
            .ToList(),
        area.CreatedAtUtc,
        area.UpdatedAtUtc);
}