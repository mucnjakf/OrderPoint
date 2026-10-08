using OrderPoint.Domain.Entities;

namespace OrderPoint.Application.Dtos.Mappers;

internal static class BartenderMapper
{
    internal static BartenderDto ToBartenderDto(this Bartender bartender) => new(
        bartender.Id,
        bartender.FirstName,
        bartender.LastName,
        bartender.Email,
        bartender.PhoneNumber,
        bartender.Status,
        bartender.Notes,
        bartender.ImageUrl,
        bartender.CreatedAtUtc,
        bartender.UpdatedAtUtc);
}