using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using OrderPoint.Admin.Categories.Api.Requests;

namespace OrderPoint.Admin.Categories.Dialogs;

public sealed partial class CreateCategoryDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private CreateCategoryRequest Request { get; set; } = new();

    private void OnValidSubmit(EditContext editContext)
    {
        StateHasChanged();

        MudDialogInstance.Close(DialogResult.Ok(Request));
    }

    private void Cancel()
    {
        MudDialogInstance.Cancel();
    }
}