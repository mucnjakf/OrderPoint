using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using OrderPoint.Admin.Categories.Api;
using OrderPoint.Admin.Categories.Dtos;
using OrderPoint.Admin.Items.Api.Requests;
using OrderPoint.Admin.Items.Dtos;

namespace OrderPoint.Admin.Items.Dialogs;

public sealed partial class UpdateItemDialog
{
    [Parameter]
    public ItemDto Item { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    [Inject]
    private CategoryApiClient CategoryApiClient { get; set; } = null!;

    private CategoryDto? SelectedCategory { get; set; }

    private bool IsFormSubmitted { get; set; }

    private UpdateItemRequest Request { get; set; } = null!;

    protected override void OnParametersSet()
    {
        InitializeRequest();
    }

    private void InitializeRequest()
    {
        Request = new UpdateItemRequest
        {
            Name = Item.Name,
            Description = Item.Description,
            Portion = Item.Portion,
            Price = Item.Price,
            ImageUrl = Item.ImageUrl,
            CategoryId = Item.Category.Id
        };

        ItemCategoryDto category = Item.Category;

        SelectedCategory = new CategoryDto(
            category.Id,
            category.Name,
            category.Description,
            category.Status,
            category.ImageUrl,
            0,
            category.CreatedAtUtc,
            category.UpdatedAtUtc);
    }

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
        StateHasChanged();

        MudDialogInstance.Close(DialogResult.Ok(Request));
    }

    private void Cancel()
    {
        MudDialogInstance.Cancel();
    }
}