using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Shared.Dtos;

namespace OrderPoint.Admin.Shared.Components;

public sealed partial class DataTimeline<TItem>
{
    private const string TimelineStyle =
        "border-top: 1px solid var(--mud-palette-lines-default); " +
        "border-bottom: 1px solid var(--mud-palette-lines-default);";

    private const string TicketStyle = "transition: all 0.2s ease-in-out;";

    private const string HoveredTicketStyle = TicketStyle + " background-color: var(--mud-palette-table-hover);";

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
    [EditorRequired]
    public Func<TItem, Color> GetItemColor { get; set; }

    [Parameter]
    [EditorRequired]
    public Func<TItem, string> GetItemIcon { get; set; }

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

    private bool IsHovered(TItem item)
    {
        return EqualityComparer<TItem>.Default.Equals(item, HoveredItem);
    }

    private string GetTicketStyle(TItem item)
    {
        return IsHovered(item) ? HoveredTicketStyle : TicketStyle;
    }
}