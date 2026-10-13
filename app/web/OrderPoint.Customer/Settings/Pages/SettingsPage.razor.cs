using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Customer.Settings.Enumerations;
using OrderPoint.Customer.Settings.Services;
using OrderPoint.Customer.Tables.Services;

namespace OrderPoint.Customer.Settings.Pages;

public sealed partial class SettingsPage : IDisposable
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
    private TableService TableService { get; set; } = null!;

    protected override void OnInitialized()
    {
        TableService.Changed += OnTableChanged;
    }

    public void Dispose()
    {
        TableService.Changed -= OnTableChanged;
    }

    private async Task OnAppearanceModeChangedAsync(AppearanceMode mode)
    {
        await AppearanceService.SetAsync(mode);
    }

    private void OnTableChanged()
    {
        _ = InvokeAsync(StateHasChanged);
    }
}