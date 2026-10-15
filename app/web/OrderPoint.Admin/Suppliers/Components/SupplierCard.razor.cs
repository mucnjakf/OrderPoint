using System.Globalization;
using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Suppliers.Dtos;
using OrderPoint.Shared.Extensions;
using OrderPoint.Shared.Services;

namespace OrderPoint.Admin.Suppliers.Components;

// A supplier's contact card: who to call, what comes from them, what to order, when they last delivered
public sealed partial class SupplierCard
{
    private const string SecondaryStyle = "color: var(--mud-palette-text-secondary)";

    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public SupplierDto Supplier { get; set; } = null!;

    private string ReorderText => Supplier.ReorderCount == 0 ? string.Empty : $" · {Supplier.ReorderCount} to reorder";

    private string LastDeliveryText
    {
        get
        {
            if (Supplier.LastDeliveredOn is not { } deliveredOn)
            {
                return "No deliveries yet";
            }

            string pattern = CultureInfo.CurrentCulture.GetShortMonthDayPattern();

            return $"Last delivery {deliveredOn.ToString(pattern, CultureInfo.CurrentCulture)}";
        }
    }
}