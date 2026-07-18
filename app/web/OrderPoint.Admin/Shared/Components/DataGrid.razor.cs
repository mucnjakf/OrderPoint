using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Shared.Dtos;

namespace OrderPoint.Admin.Shared.Components;

public sealed partial class DataGrid<TItem>
{
    [Parameter]
    [EditorRequired]
    public IReadOnlyList<TItem> Items { get; set; } = [];

    [Parameter]
    [EditorRequired]
    public PaginationDto<TItem>? Pagination { get; set; }

    [Parameter]
    [EditorRequired]
    public bool IsLoading { get; set; }

    [Parameter]
    [EditorRequired]
    public string SearchLabel { get; set; }

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

    [Parameter]
    [EditorRequired]
    public string[] SortByOptions { get; set; }

    [Parameter]
    [EditorRequired]
    public Func<string, string> GetSortByLabel { get; set; }

    [Parameter]
    [EditorRequired]
    public Func<string, string> GetSortByIcon { get; set; }

    [Parameter]
    public RenderFragment? FilterContent { get; set; }

    [Parameter]
    public string? CreateButtonText { get; set; }

    [Parameter]
    public Func<TItem, string>? DetailsButtonHref { get; set; }

    [Parameter]
    public Func<TItem, string>? UpdateButtonHref { get; set; }

    [Parameter]
    public Func<TItem, bool>? DeleteButtonDisabled { get; set; }

    [Parameter]
    public string? DeleteButtonDisabledTooltipText { get; set; }

    [Parameter]
    public EventCallback<TItem> OnDeleteClick { get; set; }

    [Parameter]
    public string? CreateButtonHref { get; set; }

    [Parameter]
    [EditorRequired]
    public RenderFragment<TItem> RowTemplate { get; set; }

    [Parameter]
    [EditorRequired]
    public string EmptyStateIcon { get; set; }

    [Parameter]
    [EditorRequired]
    public string EmptyStateText { get; set; }

    private async Task OnSearchChangedAsync()
    {
        await SearchQueryChanged.InvokeAsync(SearchQuery);
        await OnSearchChanged.InvokeAsync();
    }

    private async Task OnSortChangedAsync()
    {
        await SelectedSortByChanged.InvokeAsync(SelectedSortBy);
        await OnSortChanged.InvokeAsync();
    }

    private async Task OnDeleteClickedAsync(TItem item)
    {
        await OnDeleteClick.InvokeAsync(item);
    }
}