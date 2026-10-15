using System.Globalization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Deliveries.Dtos;
using OrderPoint.Shared.Services;

namespace OrderPoint.Admin.Deliveries.Dialogs;

public sealed partial class DeliveryDetailsDialog
{
    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Parameter]
    public DeliveryDto Delivery { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    // A delivery keeps the full date
    private string DateText => Delivery.DeliveredOn.ToString("D", CultureInfo.CurrentCulture);

    private void Close()
    {
        MudDialogInstance.Close(DialogResult.Ok(true));
    }
}