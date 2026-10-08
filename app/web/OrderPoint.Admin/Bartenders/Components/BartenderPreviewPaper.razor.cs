using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Bartenders.Enumerations;

namespace OrderPoint.Admin.Bartenders.Components;

public sealed partial class BartenderPreviewPaper
{
    [Parameter]
    [EditorRequired]
    public string? ImageUrl { get; set; }

    [Parameter]
    [EditorRequired]
    public string FirstName { get; set; }

    [Parameter]
    [EditorRequired]
    public string LastName { get; set; }

    [Parameter]
    [EditorRequired]
    public string Email { get; set; }

    [Parameter]
    [EditorRequired]
    public BartenderStatus Status { get; set; }

    [Parameter]
    public string NamePlaceholder { get; set; } = string.Empty;

    [Parameter]
    public string EmailPlaceholder { get; set; } = string.Empty;

    private string FullName => $"{FirstName} {LastName}".Trim();
}