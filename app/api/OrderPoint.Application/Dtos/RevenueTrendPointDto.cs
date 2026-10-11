namespace OrderPoint.Application.Dtos;

/// <summary>The revenue of one bucket.</summary>
/// <param name="BucketStartUtc">The start of the hour or day in the viewer's time zone, as UTC.</param>
/// <param name="Revenue">The revenue of completed orders in the bucket, in euros; 0 if there were none.</param>
public sealed record RevenueTrendPointDto(DateTimeOffset BucketStartUtc, decimal Revenue);