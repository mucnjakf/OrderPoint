using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Categories.Api;
using OrderPoint.Admin.Categories.Dtos;
using OrderPoint.Admin.Items.Api.Requests;
using OrderPoint.Admin.Shared.Services;

namespace OrderPoint.Admin.Items.Dialogs;

public sealed partial class CreateItemDialog
{
    [Parameter]
    public CategoryDto? Category { get; set; }

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private CategoryApiClient CategoryApiClient { get; set; } = null!;

    private CreateItemRequest Request { get; set; } = new();

    private CategoryDto? SelectedCategory { get; set; }

    private bool IsFormSubmitted { get; set; }

    protected override void OnInitialized()
    {
        if (Category is not null)
        {
            Request.CategoryId = Category.Id;
            SelectedCategory = Category;
        }
    }

    private async Task<IEnumerable<CategoryDto>> OnCategorySearchAsync(
        string? value,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<CategoryDto>? categories = await ApiService.ExecuteAsync(
            () => CategoryApiClient.SearchCategoriesAsync(value, cancellationToken),
            cancellationToken);

        return categories ?? [];
    }

    private void OnInvalidSubmit()
    {
        IsFormSubmitted = true;
    }

    private void OnValidSubmit()
    {
        MudDialogInstance.Close(DialogResult.Ok(Request));
    }

    private void Cancel()
    {
        MudDialogInstance.Cancel();
    }
}