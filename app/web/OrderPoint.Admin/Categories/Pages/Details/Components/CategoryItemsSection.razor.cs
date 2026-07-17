using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Items.Dtos;
using OrderPoint.Admin.Shared.Dtos;

namespace OrderPoint.Admin.Categories.Pages.Details.Components;

public sealed partial class CategoryItemsSection
{
    [Parameter]
    [EditorRequired]
    public IReadOnlyList<ItemDto> Items { get; set; } = [];

    [Parameter]
    [EditorRequired]
    public PaginationDto<ItemDto>? Pagination { get; set; }

    [Parameter]
    [EditorRequired]
    public bool IsLoading { get; set; }

    [Parameter]
    [EditorRequired]
    public string? SearchQuery { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<string?> SearchQueryChanged { get; set; }

    [Parameter]
    [EditorRequired]
    public string SelectedSortBy { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<string> SelectedSortByChanged { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback OnSearchChanged { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback OnSortChanged { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<int> OnPageChanged { get; set; }
}