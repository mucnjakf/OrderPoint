namespace OrderPoint.Application.Dtos;

public sealed record RevenueTrendPointDto(DateTimeOffset BucketStartUtc, decimal Revenue);