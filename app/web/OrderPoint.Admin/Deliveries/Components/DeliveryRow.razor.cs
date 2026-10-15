using System.Globalization;
using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Deliveries.Dtos;
using OrderPoint.Shared.Extensions;

namespace OrderPoint.Admin.Deliveries.Components;

// One delivery: who brought it, when and under which invoice, what came, what it cost
public sealed partial class DeliveryRow
{
    private const int NamedItemsCount = 3;

    [Parameter]
    [EditorRequired]
    public DeliveryDto Delivery { get; set; } = null!;

    // "15 Oct · INV-2231"
    private string DateAndReferenceText
    {
        get
        {
            string date = Delivery.DeliveredOn.ToString(
                CultureInfo.CurrentCulture.GetShortMonthDayPattern(),
                CultureInfo.CurrentCulture);

            return Delivery.Reference is null ? date : $"{date} · {Delivery.Reference}";
        }
    }

    // "Havana 3, Lager keg, Limes +2"
    private string ItemsText
    {
        get
        {
            string names = string.Join(", ", Delivery.Lines.Take(NamedItemsCount).Select(line => line.StockItemName));
            int moreCount = Delivery.Lines.Count - NamedItemsCount;

            return moreCount > 0 ? $"{names} +{moreCount}" : names;
        }
    }
}