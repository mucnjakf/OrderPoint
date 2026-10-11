using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Categories.Dtos;
using OrderPoint.Admin.Categories.Enumerations;
using OrderPoint.Admin.Shared.Extensions;

namespace OrderPoint.Admin.Categories.Dialogs;

public sealed partial class ChangeCategoriesStatusDialog
{
    [Parameter]
    public IReadOnlyList<CategoryDto> Categories { get; set; } = [];

    [Parameter]
    public CategoryStatus Status { get; set; }

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private bool IsActivating => Status is CategoryStatus.Active;

    private string Title => IsActivating ? "Activate categories" : "Deactivate categories";

    private string StatusIcon => IsActivating ? Icons.Material.Filled.CheckCircle : Icons.Material.Filled.Block;

    private Color StatusColor => IsActivating ? Color.Success : Color.Warning;

    private Severity AlertSeverity => IsActivating ? Severity.Info : Severity.Warning;

    private string AlertText => IsActivating
        ? "Items in these categories can be ordered again, unless they are inactive themselves."
        : "Items in these categories can't be ordered while the category is inactive. The items keep their own status.";

    private string ConfirmText =>
        $"{(IsActivating ? "Activate" : "Deactivate")} {Categories.Count.ToCountText("category", "categories")}";

    private void Confirm()
    {
        MudDialogInstance.Close(DialogResult.Ok(true));
    }

    private void Cancel()
    {
        MudDialogInstance.Cancel();
    }
}