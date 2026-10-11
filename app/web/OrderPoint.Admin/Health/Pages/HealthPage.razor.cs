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
        ("Blob storage", "Images in Azure Blob Storage", Icons.Material.Filled.PhotoLibrary)
    ];

    private static readonly (string Name, string Subtitle, string Icon)[] DevTools =
    [
        ("pgweb", "Database browser", Icons.Material.Filled.TableView),
        ("Storage Explorer", "Blob storage browser", Icons.Material.Filled.FolderOpen)
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