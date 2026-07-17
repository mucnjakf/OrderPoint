using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Categories.Enumerations;

namespace OrderPoint.Admin.Categories.Pages.Main.Components;

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