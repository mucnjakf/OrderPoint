using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using OrderPoint.Admin.Categories.Api;
using OrderPoint.Admin.Categories.Dtos;
using OrderPoint.Admin.Items.Api.Requests;

namespace OrderPoint.Admin.Items.Dialogs;

public sealed partial class CreateItemDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    [Inject]
    private CategoryApiClient CategoryApiClient { get; set; } = null!;

    private CreateItemRequest Request { get; set; } = new();

    private CategoryDto? SelectedCategory { get; set; }

    private bool IsFormSubmitted { get; set; }

    private async Task<IEnumerable<CategoryDto>>? OnCategorySearchAsync(
        string? value,
        CancellationToken cancellationToken)
        => await CategoryApiClient.SearchCategoriesAsync(value, cancellationToken);

    private void OnInvalidSubmit()
    {
        IsFormSubmitted = true;
        StateHasChanged();
    }

    private void OnValidSubmit(EditContext editContext)
    {
        IsFormSubmitted = true;

        StateHasChanged();

        MudDialogInstance.Close(DialogResult.Ok(Request));
    }

    private void Cancel()
    {
        MudDialogInstance.Cancel();
    }
}