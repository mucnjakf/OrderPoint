using Microsoft.AspNetCore.Components;
using OrderPoint.Shared.Dtos;

namespace OrderPoint.Admin.Shared.Components;

public sealed partial class DataGrid<TItem>
{
    // 3 rows of 4 cards
    private const int LoadingItemsCount = 12;

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
    [EditorRequired]
    public string CreateButtonText { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback OnCreateClick { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<TItem> OnDetailsClick { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<TItem> OnUpdateClick { get; set; }

    [Parameter]
    public Func<TItem, bool>? DeleteButtonDisabled { get; set; }

    [Parameter]
    public string? DeleteButtonDisabledTooltipText { get; set; }

    [Parameter]
    public EventCallback<TItem> OnDeleteClick { get; set; }

    [Parameter]
    [EditorRequired]
    public RenderFragment<TItem> RowTemplate { get; set; }

    [Parameter]
    [EditorRequired]
    public string EmptyStateIcon { get; set; }

    [Parameter]
    [EditorRequired]
    public string EmptyStateText { get; set; }

    [Parameter]
    public RenderFragment? LoadingTemplate { get; set; }

    [Parameter]
    public HashSet<TItem> SelectedItems { get; set; } = [];

    [Parameter]
    public EventCallback<HashSet<TItem>> SelectedItemsChanged { get; set; }

    [Parameter]
    public RenderFragment? BulkActionsTemplate { get; set; }

    private bool IsSelectable => SelectedItemsChanged.HasDelegate;

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

    private async Task OnCreateClickAsync()
    {
        await OnCreateClick.InvokeAsync();
    }

    private async Task OnDetailsClickAsync(TItem item)
    {
        await OnDetailsClick.InvokeAsync(item);
    }

    private async Task OnUpdateClickAsync(TItem item)
    {
        await OnUpdateClick.InvokeAsync(item);
    }

    private async Task OnDeleteClickAsync(TItem item)
    {
        await OnDeleteClick.InvokeAsync(item);
    }

    private bool IsDeleteButtonDisabled(TItem item)
    {
        return DeleteButtonDisabled?.Invoke(item) ?? false;
    }

    private string GetDeleteButtonTooltipText(TItem item)
    {
        return IsDeleteButtonDisabled(item) && DeleteButtonDisabledTooltipText is not null
            ? DeleteButtonDisabledTooltipText
            : "Delete";
    }

    private static string SelectionCheckBoxStyle =>
        "position: absolute; top: 8px; left: 8px; z-index: 1; " +
        "border-radius: var(--mud-default-borderradius); background-color: var(--mud-palette-surface);";

    private string GetCardStyle(TItem item)
    {
        return IsSelected(item)
            ? "position: relative; background-color: var(--mud-palette-table-hover);"
            : "position: relative;";
    }

    private bool IsSelected(TItem item)
    {
        return SelectedItems.Contains(item);
    }

    private async Task SetSelectedAsync(TItem item, bool isSelected)
    {
        HashSet<TItem> selectedItems = [.. SelectedItems];

        if (isSelected)
        {
            selectedItems.Add(item);
        }
        else
        {
            selectedItems.Remove(item);
        }

        await SelectedItemsChanged.InvokeAsync(selectedItems);
    }

    private async Task SelectAllAsync()
    {
        await SelectedItemsChanged.InvokeAsync([.. Items]);
    }

    private async Task ClearSelectionAsync()
    {
        await SelectedItemsChanged.InvokeAsync([]);
    }
}