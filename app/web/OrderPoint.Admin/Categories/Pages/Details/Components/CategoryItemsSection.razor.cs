using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Categories.Dtos;
using OrderPoint.Admin.Items.Api;
using OrderPoint.Admin.Items.Api.Requests;
using OrderPoint.Admin.Items.Dialogs;
using OrderPoint.Admin.Items.Dtos;
using OrderPoint.Admin.Shared.Dtos;
using OrderPoint.Admin.Shared.Services;

namespace OrderPoint.Admin.Categories.Pages.Details.Components;

public sealed partial class CategoryItemsSection
{
    [Parameter]
    [EditorRequired]
    public CategoryDto Category { get; set; }

    [Parameter]
    [EditorRequired]
    public IReadOnlyList<ItemDto> Items { get; set; } = [];

    [Parameter]
    [EditorRequired]
    public PaginationDto<ItemDto>? Pagination { get; set; }

    [Parameter]
    [EditorRequired]
    public bool IsLoading { get; set; }

    [Parameter]
    [EditorRequired]
    public string? SearchQuery { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<string?> SearchQueryChanged { get; set; }

    [Parameter]
    [EditorRequired]
    public string SelectedSortBy { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<string> SelectedSortByChanged { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback OnSearchChanged { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback OnSortChanged { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<int> OnPageChanged { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback OnItemCreated { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback OnItemUpdated { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback OnItemDeleted { get; set; }

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private ItemApiClient ItemApiClient { get; set; } = null!;

    private async Task ShowCreateItemDialogAsync()
    {
        var parameters = new DialogParameters<CreateItemDialog>
        {
            { dialog => dialog.Category, Category }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<CreateItemDialog>(string.Empty, parameters, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (!dialogResult.Canceled)
        {
            var request = (dialogResult.Data as CreateItemRequest)!;

            await ApiService.ExecuteAsync(async ()
                => await ItemApiClient.CreateItemAsync(request));

            Snackbar.Add($"Item {request.Name} created successfully", Severity.Success);

            await OnItemCreated.InvokeAsync();
        }
    }

    private async Task ShowUpdateItemDialogAsync(ItemDto item)
    {
        var parameters = new DialogParameters<UpdateItemDialog>
        {
            { dialog => dialog.Item, item }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<UpdateItemDialog>(string.Empty, parameters, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (!dialogResult.Canceled)
        {
            var request = (dialogResult.Data as UpdateItemRequest)!;

            await ApiService.ExecuteAsync(async ()
                => await ItemApiClient.UpdateItemAsync(item.Id, request));

            Snackbar.Add($"Item {request.Name} edited successfully", Severity.Success);

            await OnItemUpdated.InvokeAsync();
        }
    }

    private async Task ShowDeleteItemDialogAsync(Guid id, string itemName)
    {
        var parameters = new DialogParameters<DeleteItemDialog>
        {
            { dialog => dialog.ItemName, itemName }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<DeleteItemDialog>(string.Empty, parameters, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (!dialogResult.Canceled)
        {
            await ApiService.ExecuteAsync(async ()
                => await ItemApiClient.DeleteItemAsync(id));

            Snackbar.Add($"Item {itemName} deleted successfully", Severity.Success);

            await OnItemDeleted.InvokeAsync();
        }
    }
}