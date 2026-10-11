using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Health.Services;
using OrderPoint.Admin.Shared.Extensions;

namespace OrderPoint.Admin.Health.Components;

public sealed partial class HealthStatusIndicator : IDisposable
{
    private const string MessageBaseStyle =
        "display: inline-block; overflow: hidden; white-space: nowrap; " +
        "transition: max-width 0.3s ease, margin-right 0.3s ease;";

    [Inject]
    private HealthService HealthService { get; set; } = null!;

    private bool IsMessageVisible { get; set; }

    private Color IconColor => HealthService.Status is null ? Color.Inherit : HealthService.Status.GetColor();

    private string MessageStyle => IsMessageVisible
        ? $"{MessageBaseStyle} max-width: 240px; margin-right: 8px;"
        : $"{MessageBaseStyle} max-width: 0; margin-right: 0;";

    protected override void OnInitialized()
    {
        HealthService.Changed += OnHealthChanged;
    }

    public void Dispose()
    {
        HealthService.Changed -= OnHealthChanged;
    }

    private void ShowMessage()
    {
        IsMessageVisible = true;
    }

    private void HideMessage()
    {
        IsMessageVisible = false;
    }

    private void OnHealthChanged()
    {
        _ = InvokeAsync(StateHasChanged);
    }
}