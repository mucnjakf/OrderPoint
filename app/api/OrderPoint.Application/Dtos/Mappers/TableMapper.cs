using OrderPoint.Domain.Entities;

namespace OrderPoint.Application.Dtos.Mappers;

internal static class TableMapper
{
    internal static TableDto ToTableDto(this Table table) => new(
        table.Id,
        table.Code,
        table.AreaId,
        table.Seats,
        table.Status,
        table.Shape,
        table.X,
        table.Y,
        table.Width,
        table.Height,
        table.CreatedAtUtc,
        table.UpdatedAtUtc);

    internal static GuestTableDto ToGuestTableDto(this Table table) => new(
        table.Code,
        table.Area.Name,
        table.Status);
}