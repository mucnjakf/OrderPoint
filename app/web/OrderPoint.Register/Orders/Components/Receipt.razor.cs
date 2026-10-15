using System.Globalization;
using Microsoft.AspNetCore.Components;
using OrderPoint.Shared.Extensions;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Services;

namespace OrderPoint.Register.Orders.Components;

// A paid order as a till receipt: lines, total, how it was paid and the change
public sealed partial class Receipt
{
    private const string ReceiptStyle =
        "font-family: ui-monospace, Consolas, monospace; font-size: 13px; line-height: 1.6; " +
        "max-width: 320px; margin: 0 auto;";

    private const string TitleStyle = "text-align: center; font-size: 16px; font-weight: 700;";

    private const string CenteredStyle = "text-align: center;";

    private const string RowStyle = "display: flex; justify-content: space-between; gap: 12px;";

    private const string RuleStyle = "border: none; border-top: 1px dashed currentColor; margin: 8px 0;";

    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public OrderDto Order { get; set; } = null!;

    // A receipt keeps the full date, not "Today"
    private string PaidAtText => (Order.PaidAtUtc ?? Order.CreatedAtUtc)
        .ToTimeZone(TimeZoneService.TimeZone)
        .ToString("g", CultureInfo.CurrentCulture);
}