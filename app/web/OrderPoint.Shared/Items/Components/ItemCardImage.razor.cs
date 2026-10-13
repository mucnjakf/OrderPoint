using Microsoft.AspNetCore.Components;

namespace OrderPoint.Shared.Items.Components;

public sealed partial class ItemCardImage
{
    private const int AreaHeight = 160;

    private const int ImageHeight = 120;

    private const int PlaceholderSize = 100;

    private const string ActiveText = "Active";

    private const string InactiveText = "Inactive";

    [Parameter]
    [EditorRequired]
    public string? ImageUrl { get; set; }

    [Parameter]
    [EditorRequired]
    public bool IsActive { get; set; }

    private static string ContainerStyle => $"position: relative; overflow: hidden; height: {AreaHeight}px;";

    private string StatusText => IsActive ? ActiveText : InactiveText;

    private string InactiveImageStyle => IsActive ? string.Empty : "filter: grayscale(1); opacity: 0.6;";

    private string BackdropStyle =>
        $"position: absolute; inset: 0; background: url('{ImageUrl}') center / cover no-repeat; " +
        $"filter: blur(30px) {(IsActive ? "saturate(1.3)" : "grayscale(1)")}; opacity: 0.55; transform: scale(1.3);";
}