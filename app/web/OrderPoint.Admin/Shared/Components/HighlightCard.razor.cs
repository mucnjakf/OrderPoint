using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace OrderPoint.Admin.Shared.Components;

public sealed partial class HighlightCard
{
    [Parameter]
    [EditorRequired]
    public string Label { get; set; }

    [Parameter]
    [EditorRequired]
    public string Icon { get; set; }

    [Parameter]
    [EditorRequired]
    public Color Color { get; set; }

    [Parameter]
    [EditorRequired]
    public bool IsLoading { get; set; }

    [Parameter]
    [EditorRequired]
    public bool ShowContent { get; set; }

    [Parameter]
    public string? EmptyText { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback OnClick { get; set; }

    [Parameter]
    [EditorRequired]
    public RenderFragment ChildContent { get; set; } = null!;

    private bool IsHovered { get; set; }

    private bool IsClickable => ShowContent && !IsLoading;

    private string CursorClass => IsClickable ? "cursor-pointer" : string.Empty;

    private async Task OnCardClickAsync()
    {
        if (!IsClickable)
        {
            return;
        }

        await OnClick.InvokeAsync();
    }
}