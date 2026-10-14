using OrderPoint.Domain.Entities;

namespace OrderPoint.Application.Dtos.Mappers;

internal static class TableRequestMapper
{
    // The code comes separately, because a new request has no table loaded
    internal static TableRequestDto ToTableRequestDto(this TableRequest request, string tableCode) => new(
        request.Id,
        request.TableId,
        tableCode,
        request.Type,
        request.Status,
        request.CreatedAtUtc,
        request.DoneAtUtc);
}