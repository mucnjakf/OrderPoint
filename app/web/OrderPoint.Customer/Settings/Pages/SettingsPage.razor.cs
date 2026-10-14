using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Customer.Orders.Services;
using OrderPoint.Customer.Settings.Dialogs;
using OrderPoint.Customer.Settings.Enumerations;
using OrderPoint.Customer.Settings.Services;

namespace OrderPoint.Customer.Settings.Pages;

public sealed partial class SettingsPage
{
    private static readonly (AppearanceMode Mode, string Icon)[] AppearanceModes =
    [
        (AppearanceMode.Auto, Icons.Material.Filled.BrightnessAuto),
        (AppearanceMode.Light, Icons.Material.Filled.LightMode),
        (AppearanceMode.Dark, Icons.Material.Filled.DarkMode)
    ];

    [Inject]
    private AppearanceService AppearanceService { get; set; } = null!;

    [Inject]
    private MyOrdersService MyOrdersService { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    private async Task OnAppearanceModeChangedAsync(AppearanceMode mode)
    {
        await AppearanceService.SetAsync(mode);
    }

    // Only hides them on this phone; the orders themselves stay with the bar
    private async Task ForgetOrdersAsync()
    {
        var parameters = new DialogParameters<ForgetOrdersDialog>
        {
            { dialog => dialog.OrdersCount, MyOrdersService.OrderIds.Count }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.ExtraSmall,
            FullWidth = true
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<ForgetOrdersDialog>(string.Empty, parameters, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        await MyOrdersService.ClearAsync();
    }
}