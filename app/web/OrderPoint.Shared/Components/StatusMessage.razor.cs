using Microsoft.AspNetCore.Components;

namespace OrderPoint.Shared.Components;

public sealed partial class StatusMessage
{
    [Parameter]
    [EditorRequired]
    public string Icon { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public string Title { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public string Text { get; set; } = null!;

    [Parameter]
    public string? Caption { get; set; }

    // The button goes back to the app's home page ("/")
    [Parameter]
    [EditorRequired]
    public string ButtonText { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public string ButtonIcon { get; set; } = null!;
}