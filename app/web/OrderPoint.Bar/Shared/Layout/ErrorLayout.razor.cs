using MudBlazor;
using OrderPoint.Shared.Layout;

namespace OrderPoint.Bar.Shared.Layout;

// Used by the static error page, where MainLayout cannot run because browser storage is unavailable
public sealed partial class ErrorLayout
{
    private MudTheme Theme { get; } = AppTheme.Create();
}