using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace OrderPoint.Admin.Features.Shared.Components;

public sealed partial class PageHeader
{
    [Parameter]
    [EditorRequired]
    public IReadOnlyList<BreadcrumbItem> Breadcrumbs { get; set; }

    [Parameter]
    [EditorRequired]
    public string Text { get; set; }
}