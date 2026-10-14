namespace OrderPoint.Customer.Tables.Dtos;

// What the browser keeps about the table: its code, its area and when its QR code was scanned
public sealed record TableVisitDto(string TableCode, string? AreaName, DateTimeOffset ScannedAtUtc);