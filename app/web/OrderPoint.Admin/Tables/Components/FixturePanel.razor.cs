using Microsoft.AspNetCore.Components;
using OrderPoint.Shared.Tables.Dtos;

namespace OrderPoint.Admin.Tables.Components;

// The selected bar counter, entrance or wall
public sealed partial class FixturePanel
{
    [Parameter]
    [EditorRequired]
    public FixtureDto Fixture { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public EventCallback OnEdit { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback OnRotate { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback OnDelete { get; set; }
}