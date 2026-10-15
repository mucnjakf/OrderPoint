using System.Globalization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Dashboard.Dtos;

namespace OrderPoint.Admin.Dashboard.Components;

// The period's revenue split into cash, card at the register and card online, each with its share
public sealed partial class PaymentMethodsSummary
{
    private const double FullPercent = 100;

    [Parameter]
    [EditorRequired]
    public PaymentMethodsDto PaymentMethods { get; set; } = null!;

    private decimal Total => PaymentMethods.CashTotal + PaymentMethods.CardTotal + PaymentMethods.OnlineCardTotal;

    private IReadOnlyList<(string Label, string Icon, Color Color, decimal Total, int Count)> Methods =>
    [
        ("Cash", Icons.Material.Filled.Payments, Color.Success, PaymentMethods.CashTotal, PaymentMethods.CashCount),
        ("Card at the register", Icons.Material.Filled.CreditCard, Color.Info, PaymentMethods.CardTotal,
            PaymentMethods.CardCount),
        ("Card online", Icons.Material.Filled.PhoneIphone, Color.Primary, PaymentMethods.OnlineCardTotal,
            PaymentMethods.OnlineCardCount)
    ];

    private double GetSharePercent(decimal total)
    {
        return Total == 0 ? 0 : (double)(total / Total) * FullPercent;
    }

    private string GetShareText(decimal total)
    {
        return (Total == 0 ? 0 : total / Total).ToString("P0", CultureInfo.CurrentCulture);
    }
}