using Microsoft.AspNetCore.Components;
using OrderPoint.Shared.Dtos;

namespace OrderPoint.Admin.Shared.Components;

// A read-only ledger: compact rows split by lines, one of them selected, with an optional sticky panel beside the list
public sealed partial class DataLedger<TItem>
{
    private const int LoadingRowsCount = 3;

    private const string RowStyle =
        "padding: 12px 16px; border-bottom: 1px solid var(--mud-palette-lines-default); " +
        "transition: background-color 0.2s ease-in-out;";

    private const string HoveredRowStyle = RowStyle + " background-color: var(--mud-palette-table-hover);";

    private const string SelectedRowStyle =
        HoveredRowStyle + " box-shadow: inset 3px 0 0 var(--mud-palette-primary);";

    private const string WithSideContentStyle =
        "display: grid; grid-template-columns: minmax(0, 1fr) 380px; gap: 12px; align-items: start;";

    private const string SideContentStyle = "position: sticky; top: 80px;";

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
    public Func<TItem, bool> IsItemSelected { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<TItem> OnItemClick { get; set; }

    [Parameter]
    [EditorRequired]
    public RenderFragment<TItem> ItemTemplate { get; set; }

    [Parameter]
    [EditorRequired]
    public string EmptyStateIcon { get; set; }

    [Parameter]
    [EditorRequired]
    public string EmptyStateText { get; set; }

    [Parameter]
    public RenderFragment? LoadingTemplate { get; set; }

    // Starts level with the list, below the toolbar
    [Parameter]
    public RenderFragment? SideContent { get; set; }

    private TItem? HoveredItem { get; set; }

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

    private async Task OnItemClickAsync(TItem item)
    {
        await OnItemClick.InvokeAsync(item);
    }

    private string GetRowStyle(TItem item)
    {
        if (IsItemSelected(item))
        {
            return SelectedRowStyle;
        }

        return EqualityComparer<TItem>.Default.Equals(item, HoveredItem) ? HoveredRowStyle : RowStyle;
    }
}