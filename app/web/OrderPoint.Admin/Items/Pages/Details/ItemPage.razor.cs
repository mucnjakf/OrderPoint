using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Items.Api;
using OrderPoint.Admin.Items.Dialogs;
using OrderPoint.Admin.Items.Dtos;
using OrderPoint.Admin.Shared.Services;

namespace OrderPoint.Admin.Items.Pages.Details;

public sealed partial class ItemPage
{
    [Parameter]
    public Guid Id { get; set; }

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private ItemApiClient ItemApiClient { get; set; } = null!;

    private List<BreadcrumbItem> Breadcrumbs { get; set; } =
    [
        new("Dashboard", href: "/", icon: Icons.Material.Filled.Dashboard),
        new("Items", href: "/items", icon: Icons.Material.Filled.MenuBook),
        new("Details", href: null, disabled: true, icon: Icons.Material.Filled.Notes)
    ];

    private ItemDto Item { get; set; } = null!;

    private bool IsLoading { get; set; } = true;

    protected override async Task OnParametersSetAsync()
    {
        await GetItemAsync();
    }

    private async Task GetItemAsync()
    {
        IsLoading = true;

        Item = await ApiService.ExecuteAsync(async ()
            => await ItemApiClient.GetItemAsync(Id));

        IsLoading = false;
    }

    private async Task ShowDeleteItemDialogAsync()
    {
        var parameters = new DialogParameters<DeleteItemDialog>
        {
            { dialog => dialog.ItemName, Item.Name }
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
                => await ItemApiClient.DeleteItemAsync(Item.Id));

            Snackbar.Add($"Item {Item.Name} deleted successfully", Severity.Success);

            NavigationManager.NavigateTo("/items");
        }
    }
}