namespace OrderPoint.Shared.Tables.Dtos;

public sealed record AreaDto(
    Guid Id,
    string Name,
    int Columns,
    int Rows,
    int DisplayOrder,
    IReadOnlyList<TableDto> Tables,
    IReadOnlyList<FixtureDto> Fixtures,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);