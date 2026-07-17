using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Features.Categories.Enumerations;

namespace OrderPoint.Admin.Features.Categories.Components;

public sealed partial class CategoriesTableFilter
{
    [Parameter]
    [EditorRequired]
    public CategoryStatus? SelectedStatus { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<CategoryStatus?> SelectedStatusChanged { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback OnStatusChanged { get; set; }

    private async Task OnStatusChangedAsync()
    {
        await SelectedStatusChanged.InvokeAsync(SelectedStatus);
        await OnStatusChanged.InvokeAsync();
    }
}