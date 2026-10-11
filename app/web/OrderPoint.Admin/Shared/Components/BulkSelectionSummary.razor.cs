using Microsoft.AspNetCore.Components;

namespace OrderPoint.Admin.Shared.Components;

public sealed partial class BulkSelectionSummary<TItem>
{
    private const int MaxVisibleItemsCount = 5;

    [Parameter]
    [EditorRequired]
    public IReadOnlyList<TItem> Items { get; set; } = [];

    [Parameter]
    [EditorRequired]
    public RenderFragment<TItem> ItemTemplate { get; set; } = null!;

    private int HiddenItemsCount => Math.Max(Items.Count - MaxVisibleItemsCount, 0);
}