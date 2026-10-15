using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace OrderPoint.Admin.Items.Components;

public sealed partial class ItemPreviewPaper
{
    [Parameter]
    [EditorRequired]
    public string? ImageUrl { get; set; }

    [Parameter]
    [EditorRequired]
    public string Name { get; set; }

    [Parameter]
    [EditorRequired]
    public string Description { get; set; }

    [Parameter]
    [EditorRequired]
    public double Portion { get; set; }

    [Parameter]
    [EditorRequired]
    public decimal Price { get; set; }

    [Parameter]
    [EditorRequired]
    public bool IsActive { get; set; }

    // An active item whose ingredient ran short
    [Parameter]
    public bool IsSoldOut { get; set; }

    [Parameter]
    public string NamePlaceholder { get; set; } = string.Empty;

    [Parameter]
    public string DescriptionPlaceholder { get; set; } = string.Empty;

    private string StatusText => !IsActive ? "Inactive" : IsSoldOut ? "Sold out" : "Active";

    private Color StatusColor => !IsActive ? Color.Error : IsSoldOut ? Color.Warning : Color.Success;
}