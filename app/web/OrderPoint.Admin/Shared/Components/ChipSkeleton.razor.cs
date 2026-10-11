using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace OrderPoint.Admin.Shared.Components;

public sealed partial class ChipSkeleton
{
    [Parameter]
    public Size Size { get; set; } = Size.Medium;

    [Parameter]
    public string Width { get; set; } = "72px";

    [Parameter]
    public string Class { get; set; } = "ma-1";

    private string Height => Size is Size.Small ? "24px" : "32px";

    private string BorderRadiusStyle => Size is Size.Small ? "border-radius: 12px;" : "border-radius: 16px;";
}