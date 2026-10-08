using Microsoft.AspNetCore.Components;

namespace OrderPoint.Admin.Items.Components;

public sealed partial class ItemCardImage
{
    private const int AreaHeight = 160;

    private const int ImageHeight = 120;

    private const int PlaceholderSize = 100;

    [Parameter]
    [EditorRequired]
    public string? ImageUrl { get; set; }

    private static string ContainerStyle => $"position: relative; overflow: hidden; height: {AreaHeight}px;";

    private string BackdropStyle =>
        $"position: absolute; inset: 0; background: url('{ImageUrl}') center / cover no-repeat; " +
        "filter: blur(30px) saturate(1.3); opacity: 0.55; transform: scale(1.3);";
}