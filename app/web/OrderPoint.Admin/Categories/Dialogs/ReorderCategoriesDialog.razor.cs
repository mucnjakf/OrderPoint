using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Categories.Api;
using OrderPoint.Admin.Categories.Api.Requests;
using OrderPoint.Admin.Categories.Dtos;
using OrderPoint.Admin.Categories.Sorting;
using OrderPoint.Admin.Shared.Dtos;
using OrderPoint.Admin.Shared.Services;

namespace OrderPoint.Admin.Categories.Dialogs;

public sealed partial class ReorderCategoriesDialog
{
    private const int MaxCategoriesCount = 100;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private CategoryApiClient CategoryApiClient { get; set; } = null!;

    private MudDropContainer<CategoryDto> DropContainer { get; set; } = null!;

    private IReadOnlyList<CategoryDto> OriginalCategories { get; set; } = [];

    private List<CategoryDto> Categories { get; set; } = [];

    private bool IsLoading { get; set; } = true;

    private bool IsOrderChanged => !Categories.SequenceEqual(OriginalCategories);

    protected override async Task OnInitializedAsync()
    {
        PaginationDto<CategoryDto>? pagination = await ApiService.ExecuteAsync(
            () => CategoryApiClient.GetCategoriesAsync(
                pageNumber: 1,
                MaxCategoriesCount,
                nameof(CategorySortBy.DisplayOrderAsc)));

        OriginalCategories = pagination?.Items ?? [];
        Categories = [.. OriginalCategories];

        IsLoading = false;
    }

    private int GetPosition(CategoryDto category)
    {
        return Categories.IndexOf(category) + 1;
    }

    private void OnCategoryDropped(MudItemDropInfo<CategoryDto> dropInfo)
    {
        if (dropInfo.Item is null)
        {
            return;
        }

        Categories.Remove(dropInfo.Item);
        Categories.Insert(Math.Min(dropInfo.IndexInZone, Categories.Count), dropInfo.Item);

        DropContainer.Refresh();
    }

    private void Confirm()
    {
        var request = new ReorderCategoriesRequest
        {
            Ids = [.. Categories.Select(category => category.Id)]
        };

        MudDialogInstance.Close(DialogResult.Ok(request));
    }

    private void Cancel()
    {
        MudDialogInstance.Cancel();
    }
}