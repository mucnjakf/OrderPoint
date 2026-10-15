using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Shared.Extensions;
using OrderPoint.Admin.Stock.Api.Requests;
using OrderPoint.Admin.Stock.Dtos;

namespace OrderPoint.Admin.Stock.Dialogs;

// Waste is usually small, so it starts in the base unit: 50 ml rather than 0.07 bottles
public sealed partial class RecordStockWasteDialog
{
    private static readonly string[] Reasons = ["Spilled", "Broken", "Expired", "Given away"];

    [Parameter]
    public StockItemDto StockItem { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private RecordStockWasteRequest Request { get; set; } = null!;

    private string AfterText => StockItem.GetQuantityText(StockItem.Quantity - Request.Amount);

    protected override void OnInitialized()
    {
        Request = new RecordStockWasteRequest
        {
            PackSize = StockItem.PackSize,
            IsInPacks = StockItem.PackSize == 1
        };
    }

    private void OnValidSubmit()
    {
        MudDialogInstance.Close(DialogResult.Ok(Request));
    }

    private void Cancel()
    {
        MudDialogInstance.Cancel();
    }
}