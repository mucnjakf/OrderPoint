using Microsoft.AspNetCore.Components;

namespace OrderPoint.Shared.Components;

// Title and description, with the section's control below them at full width (phones have no room beside them)
public sealed partial class SettingsSection
{
    [Parameter]
    [EditorRequired]
    public string Icon { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public string Title { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public string Description { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public RenderFragment ChildContent { get; set; } = null!;
}