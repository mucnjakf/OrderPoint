using System.Globalization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Categories.Api;
using OrderPoint.Admin.Categories.Dtos;
using OrderPoint.Admin.Categories.Enumerations;
using OrderPoint.Admin.Items.Api.Requests;
using OrderPoint.Admin.Items.Dtos;
using OrderPoint.Admin.Items.Enumerations;
using OrderPoint.Admin.Shared.Extensions;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Extensions;
using OrderPoint.Shared.Services;

namespace OrderPoint.Admin.Items.Dialogs;

public sealed partial class UpdateItemDialog
{
    private const string CurrentImageFileName = "Current image";

    private const decimal MinPercent = -100;

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

    private MudAutocomplete<CategoryDto> CategoryAutocomplete { get; set; } = null!;

    private bool IsPreviewActive =>
        Request.Status == ItemStatus.Active && SelectedCategory?.Status != CategoryStatus.Inactive;

    private bool IsFormSubmitted { get; set; }

    // Same modes and rules as the bulk "Adjust prices"; the request carries the resulting price
    private PriceAdjustmentMode PriceMode { get; set; } = PriceAdjustmentMode.Set;

    private decimal? PriceValue { get; set; }

    private decimal NewPrice => PriceValue is null ? Item.Price : PriceMode.Apply(Item.Price, PriceValue.Value);

    private string PriceAdornmentText => PriceMode is PriceAdjustmentMode.ChangeByPercent ? "%" : "€";

    private string PriceHelperText => PriceMode is PriceAdjustmentMode.Set
        ? "Price in euros."
        : $"{FormatPrice(Item.Price)} → {FormatPrice(NewPrice)}";

    private string? PriceErrorText => PriceValue switch
    {
        null when PriceMode is PriceAdjustmentMode.Set => "Price is required.",
        null => null,
        <= MinPercent when PriceMode is PriceAdjustmentMode.ChangeByPercent => "Value must be greater than -100.",
        _ when NewPrice <= 0 => "Price must be positive.",
        _ => null
    };

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
            Status = Item.Status,
            CategoryId = Item.Category.Id
        };

        PriceValue = Item.Price;

        ItemCategoryDto category = Item.Category;

        SelectedCategory = new CategoryDto(
            category.Id,
            category.Name,
            category.Description,
            category.Status,
            DisplayOrder: 0,
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

    private async Task OnCategoryClearedAsync()
    {
        await CategoryAutocomplete.OpenMenuAsync();
    }

    private void OnSelectedCategoryChanged()
    {
        Request.CategoryId = SelectedCategory?.Id ?? Guid.Empty;
    }

    private static string FormatPrice(decimal price)
    {
        return price.ToString("C", CultureInfo.CurrentCulture);
    }

    // Set starts from the price so far, a change starts empty (no change yet)
    private void OnPriceModeChanged()
    {
        PriceValue = PriceMode is PriceAdjustmentMode.Set ? Request.Price : null;

        OnPriceValueChanged();
    }

    private void OnPriceValueChanged()
    {
        Request.Price = NewPrice;
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
        if (PriceErrorText is not null)
        {
            return;
        }

        MudDialogInstance.Close(DialogResult.Ok(Request));
    }

    private void Cancel()
    {
        MudDialogInstance.Cancel();
    }
}