using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Application.Dtos;

/// <summary>A table as the customer app sees it after a guest scans its QR code.</summary>
/// <param name="Code">The code, e.g. `T7`.</param>
/// <param name="AreaName">The name of the area the table is in, e.g. `Terrace`.</param>
/// <param name="Status">The status. An inactive table takes no orders.</param>
public sealed record GuestTableDto(
    string Code,
    string AreaName,
    TableStatus Status);