using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Categories.Api;
using OrderPoint.Admin.Categories.Dtos;
using OrderPoint.Admin.Items.Api.Requests;
using OrderPoint.Admin.Items.Dtos;
using OrderPoint.Admin.Shared.Dtos;
using OrderPoint.Admin.Shared.Extensions;
using OrderPoint.Admin.Shared.Services;

namespace OrderPoint.Admin.Items.Dialogs;

public sealed partial class UpdateItemDialog
{
    private const string CurrentImageFileName = "Current image";

    [Parameter]
    public ItemDto Item { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private CategoryApiClient CategoryApiClient { get; set; } = null!;

    private UpdateItemRequest Request { get; set; } = null!;

    private CategoryDto? SelectedCategory { get; set; }

    private bool IsFormSubmitted { get; set; }

    private string? PreviewImageUrl => Request.Image is not null
        ? Request.Image.ToDataUrl()
        : Request.RemoveImage ? null : Item.ImageUrl;

    private string? ImageFileName => Request.Image is not null
        ? Request.Image.FileName
        : PreviewImageUrl is null ? null : CurrentImageFileName;

    protected override void OnInitialized()
    {
        Request = new UpdateItemRequest
        {
            Name = Item.Name,
            Description = Item.Description,
            Portion = Item.Portion,
            Price = Item.Price,
            CategoryId = Item.Category.Id
        };

        ItemCategoryDto category = Item.Category;

        SelectedCategory = new CategoryDto(
            category.Id,
            category.Name,
            category.Description,
            category.Status,
            category.ImageUrl,
            ItemsCount: 0,
            category.CreatedAtUtc,
            category.UpdatedAtUtc);
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

    private void OnSelectedCategoryChanged()
    {
        Request.CategoryId = SelectedCategory?.Id ?? Guid.Empty;
    }

    private void OnImageSelected(ImageFileDto image)
    {
        Request.Image = image;
        Request.RemoveImage = false;
    }

    private void OnImageRemoved()
    {
        Request.Image = null;
        Request.RemoveImage = true;
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