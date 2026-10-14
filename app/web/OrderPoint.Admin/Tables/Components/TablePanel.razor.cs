using System.Text;
using Microsoft.AspNetCore.Components;
using OrderPoint.Shared.Tables.Dtos;

namespace OrderPoint.Admin.Tables.Components;

// The selected table: its details, its QR code to test or download, and what can be done with it
public sealed partial class TablePanel
{
    // White behind the code in both themes, so phones can scan it from the screen
    private const string QrCodeStyle =
        "width: 200px; align-self: center; padding: 8px; background-color: white; " +
        "border-radius: var(--mud-default-borderradius);";

    [Parameter]
    [EditorRequired]
    public TableDto Table { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public string? QrCodeUrl { get; set; }

    [Parameter]
    [EditorRequired]
    public string? QrCodeSvg { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback OnEdit { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback OnRotate { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback OnDelete { get; set; }

    private string QrCodeDataUrl =>
        $"data:image/svg+xml;base64,{Convert.ToBase64String(Encoding.UTF8.GetBytes(QrCodeSvg ?? string.Empty))}";
}