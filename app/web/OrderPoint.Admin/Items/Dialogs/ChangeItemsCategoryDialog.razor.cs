using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Categories.Api;
using OrderPoint.Admin.Categories.Dtos;
using OrderPoint.Admin.Items.Api.Requests;
using OrderPoint.Admin.Items.Dtos;
using OrderPoint.Shared.Services;

namespace OrderPoint.Admin.Items.Dialogs;

public sealed partial class ChangeItemsCategoryDialog
{
    [Parameter]
    public IReadOnlyList<ItemDto> Items { get; set; } = [];

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private CategoryApiClient CategoryApiClient { get; set; } = null!;

    private CategoryDto? SelectedCategory { get; set; }

    private MudAutocomplete<CategoryDto> CategoryAutocomplete { get; set; } = null!;

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

    private void Confirm()
    {
        if (SelectedCategory is null)
        {
            return;
        }

        var request = new ChangeItemsCategoryRequest
        {
            Ids = [.. Items.Select(item => item.Id)],
            CategoryId = SelectedCategory.Id,
            CategoryName = SelectedCategory.Name
        };

        MudDialogInstance.Close(DialogResult.Ok(request));
    }

    private void Cancel()
    {
        MudDialogInstance.Cancel();
    }
}