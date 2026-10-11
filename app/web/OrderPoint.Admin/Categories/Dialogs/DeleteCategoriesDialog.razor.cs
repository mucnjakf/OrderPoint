using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Categories.Dtos;

namespace OrderPoint.Admin.Categories.Dialogs;

public sealed partial class DeleteCategoriesDialog
{
    [Parameter]
    public IReadOnlyList<CategoryDto> Categories { get; set; } = [];

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private void Confirm()
    {
        MudDialogInstance.Close(DialogResult.Ok(true));
    }

    private void Cancel()
    {
        MudDialogInstance.Cancel();
    }
}