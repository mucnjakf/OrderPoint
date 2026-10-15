using System.Globalization;
using OrderPoint.Domain.Entities;

namespace OrderPoint.Application.Dtos.Mappers;

internal static class AreaMapper
{
    // T2 before T10: the numbers in a code compare as numbers
    private static readonly StringComparer TableCodeComparer =
        StringComparer.Create(CultureInfo.InvariantCulture, CompareOptions.NumericOrdering);

    internal static AreaDto ToAreaDto(this Area area) => new(
        area.Id,
        area.Name,
        area.Columns,
        area.Rows,
        area.DisplayOrder,
        area.Tables
            .OrderBy(table => table.Code, TableCodeComparer)
            .Select(table => table.ToTableDto())
            .ToList(),
        area.Fixtures
            .OrderBy(fixture => fixture.CreatedAtUtc)
            .Select(fixture => fixture.ToFixtureDto())
            .ToList(),
        area.CreatedAtUtc,
        area.UpdatedAtUtc);
}