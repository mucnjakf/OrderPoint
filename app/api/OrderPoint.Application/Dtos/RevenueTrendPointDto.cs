namespace OrderPoint.Application.Dtos;

/// <summary>The revenue of one bucket.</summary>
/// <param name="BucketStartUtc">The start of the hour or day in the viewer's time zone, as UTC.</param>
/// <param name="Revenue">What was paid in the bucket, in euros; 0 if nothing was.</param>
public sealed record RevenueTrendPointDto(DateTimeOffset BucketStartUtc, decimal Revenue);