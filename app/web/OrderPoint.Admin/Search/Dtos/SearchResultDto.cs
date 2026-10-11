using OrderPoint.Admin.Search.Enumerations;

namespace OrderPoint.Admin.Search.Dtos;

public sealed record SearchResultDto(
    SearchResultKind Kind,
    string Title,
    string? Subtitle = null,
    string? Meta = null,
    Guid? Id = null,
    string? ImageUrl = null,
    int? OrderNumber = null,
    string? Href = null,
    string? Icon = null);