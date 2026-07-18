using Microsoft.AspNetCore.Components;

namespace OrderPoint.Admin.Items.Pages.Shared;

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
    public decimal Price { get; set; }

    [Parameter]
    public string NamePlaceholder { get; set; } = string.Empty;

    [Parameter]
    public string DescriptionPlaceholder { get; set; } = string.Empty;
}