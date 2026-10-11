using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Search.Dtos;

namespace OrderPoint.Admin.Search.Components;

public sealed partial class SearchResultGroup
{
    private const int SkeletonRowsCount = 2;

    [Parameter]
    [EditorRequired]
    public string Title { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public IReadOnlyList<SearchResultDto> Results { get; set; } = [];

    [Parameter]
    public bool IsLoading { get; set; }

    [Parameter]
    public SearchResultDto? ActiveResult { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<SearchResultDto> OnResultClick { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<SearchResultDto> OnResultMouseEnter { get; set; }
}