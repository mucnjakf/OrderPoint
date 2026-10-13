using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Health.Dtos;
using OrderPoint.Admin.Health.Services;

namespace OrderPoint.Admin.Health.Pages;

public sealed partial class HealthPage : IDisposable
{
    // Names as the API reports them
    private static readonly (string Name, string Subtitle, string Icon)[] CoreServices =
    [
        (HealthService.ApiCheckName, "OrderPoint API", Icons.Material.Filled.Api),
        ("Database", "PostgreSQL", Icons.Material.Filled.Storage),
        ("Storage", "Azure Storage", Icons.Material.Filled.PhotoLibrary),
        ("Cache", "Redis", Icons.Material.Filled.Memory)
    ];

    private static readonly (string Name, string Subtitle, string Icon)[] Apps =
    [
        ("Bartender web", "OrderPoint Bartender", Icons.Material.Filled.LocalBar),
        ("Customer web", "OrderPoint Customer", Icons.Material.Filled.PhoneIphone)
    ];

    private static readonly (string Name, string Subtitle, string Icon)[] DevTools =
    [
        ("Database browser", "pgweb", Icons.Material.Filled.TableView),
        ("Storage browser", "Azure Storage Explorer", Icons.Material.Filled.FolderOpen),
        ("Cache browser", "RedisInsight", Icons.Material.Filled.ManageSearch)
    ];

    [Inject]
    private HealthService HealthService { get; set; } = null!;

    private bool IsLoading => HealthService.CheckedAtUtc is null;

    protected override async Task OnInitializedAsync()
    {
        HealthService.Changed += OnHealthChanged;

        await HealthService.CheckAsync();
    }

    public void Dispose()
    {
        HealthService.Changed -= OnHealthChanged;
    }

    private HealthCheckDto? GetCheck(string name)
    {
        return HealthService.Checks.FirstOrDefault(check => check.Name == name);
    }

    private void OnHealthChanged()
    {
        _ = InvokeAsync(StateHasChanged);
    }
}