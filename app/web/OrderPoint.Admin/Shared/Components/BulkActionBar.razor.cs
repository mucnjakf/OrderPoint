using Microsoft.AspNetCore.Components;

namespace OrderPoint.Admin.Shared.Components;

public sealed partial class BulkActionBar
{
    [Parameter]
    [EditorRequired]
    public int SelectedCount { get; set; }

    [Parameter]
    [EditorRequired]
    public int TotalCount { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback OnSelectAllClick { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback OnClearClick { get; set; }

    [Parameter]
    [EditorRequired]
    public RenderFragment ChildContent { get; set; } = null!;

    private async Task OnSelectAllClickAsync()
    {
        await OnSelectAllClick.InvokeAsync();
    }

    private async Task OnClearClickAsync()
    {
        await OnClearClick.InvokeAsync();
    }
}