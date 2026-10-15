namespace OrderPoint.Application.Dtos;

/// <summary>What a supplier delivered.</summary>
/// <param name="Id">The id.</param>
/// <param name="Supplier">Who delivered it.</param>
/// <param name="DeliveredOn">The day the goods came.</param>
/// <param name="Reference">The invoice or delivery note number, or `null`.</param>
/// <param name="Note">A note, or `null`.</param>
/// <param name="Lines">The stock items that came, by name.</param>
/// <param name="Total">What it cost, in euros.</param>
/// <param name="CreatedAtUtc">When it was recorded (UTC).</param>
public sealed record DeliveryDto(
    Guid Id,
    DeliverySupplierDto Supplier,
    DateOnly DeliveredOn,
    string? Reference,
    string? Note,
    IReadOnlyList<DeliveryLineDto> Lines,
    decimal Total,
    DateTimeOffset CreatedAtUtc);