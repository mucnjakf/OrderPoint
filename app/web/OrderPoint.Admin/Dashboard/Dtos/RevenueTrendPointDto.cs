namespace OrderPoint.Admin.Dashboard.Dtos;

public sealed record RevenueTrendPointDto(DateTimeOffset BucketStartUtc, decimal Revenue);