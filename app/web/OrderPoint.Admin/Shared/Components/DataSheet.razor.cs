using Microsoft.AspNetCore.Components;
using OrderPoint.Shared.Dtos;

namespace OrderPoint.Admin.Shared.Components;

// A sheet of rows under a header per group, like a printed stock sheet; the items come sorted by group, so a group's
// rows follow each other (and continue under a repeated header on the next page)
public sealed partial class DataSheet<TItem>
{
    private const int LoadingRowsCount = 3;

    private const string LineStyle = "border-bottom: 1px solid var(--mud-palette-lines-default);";

    private const string HeaderRowStyle = $"padding: 8px 16px; {LineStyle}";

    private const string GroupHeaderStyle =
        $"padding: 10px 16px; background-color: rgba(var(--mud-palette-text-primary-rgb), 0.04); {LineStyle}";

    private const string RowStyle = $"padding: 10px 16px; {LineStyle} transition: background-color 0.2s ease-in-out;";

    private const string HoveredRowStyle = RowStyle + " background-color: var(--mud-palette-table-hover);";

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
    public Func<TItem, object> GetGroupKey { get; set; }

    // Receives the group's rows on this page, e.g. for a count and a total
    [Parameter]
    [EditorRequired]
    public RenderFragment<IReadOnlyList<TItem>> GroupHeaderTemplate { get; set; }

    // The column titles, laid out like the rows
    [Parameter]
    [EditorRequired]
    public RenderFragment HeaderContent { get; set; }

    [Parameter]
    [EditorRequired]
    public RenderFragment<TItem> RowTemplate { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<TItem> OnRowClick { get; set; }

    [Parameter]
    [EditorRequired]
    public string EmptyStateIcon { get; set; }

    [Parameter]
    [EditorRequired]
    public string EmptyStateText { get; set; }

    [Parameter]
    public RenderFragment? LoadingTemplate { get; set; }

    private TItem? HoveredItem { get; set; }

    private IReadOnlyList<IReadOnlyList<TItem>> Groups
    {
        get
        {
            List<List<TItem>> groups = [];

            foreach (TItem item in Items)
            {
                if (groups.Count == 0 || !Equals(GetGroupKey(groups[^1][0]), GetGroupKey(item)))
                {
                    groups.Add([]);
                }

                groups[^1].Add(item);
            }

            return groups;
        }
    }

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

    private async Task OnRowClickAsync(TItem item)
    {
        await OnRowClick.InvokeAsync(item);
    }

    private string GetRowStyle(TItem item)
    {
        return EqualityComparer<TItem>.Default.Equals(item, HoveredItem) ? HoveredRowStyle : RowStyle;
    }
}