using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using OrderPoint.Admin.Categories.Api.Requests;
using OrderPoint.Admin.Categories.Dtos;

namespace OrderPoint.Admin.Categories.Dialogs;

public sealed partial class UpdateCategoryDialog
{
    [Parameter]
    public CategoryDto Category { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private UpdateCategoryRequest Request { get; set; } = null!;

    protected override void OnParametersSet()
    {
        InitializeRequest();
    }

    private void InitializeRequest()
    {
        Request = new UpdateCategoryRequest
        {
            Name = Category.Name,
            Description = Category.Description,
            Status = Category.Status,
            ImageUrl = Category.ImageUrl
        };
    }

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