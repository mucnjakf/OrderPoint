using OrderPoint.Domain.Entities;

namespace OrderPoint.Application.Dtos.Mappers;

internal static class BartenderMapper
{
    internal static BartenderDto ToBartenderDto(this Bartender bartender, int ordersCount) => new(
        bartender.Id,
        bartender.FirstName,
        bartender.LastName,
        bartender.Email,
        bartender.PhoneNumber,
        bartender.Status,
        bartender.Notes,
        bartender.ImageUrl,
        ordersCount,
        bartender.CreatedAtUtc,
        bartender.UpdatedAtUtc);
}