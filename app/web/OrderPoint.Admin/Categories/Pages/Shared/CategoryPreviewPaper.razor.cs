using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Categories.Enumerations;

namespace OrderPoint.Admin.Categories.Pages.Shared;

public sealed partial class CategoryPreviewPaper
{
    [Parameter]
    [EditorRequired]
    public string Name { get; set; }

    [Parameter]
    [EditorRequired]
    public string Description { get; set; }

    [Parameter]
    [EditorRequired]
    public CategoryStatus Status { get; set; }

    [Parameter]
    public string NamePlaceholder { get; set; } = string.Empty;

    [Parameter]
    public string DescriptionPlaceholder { get; set; } = string.Empty;
}