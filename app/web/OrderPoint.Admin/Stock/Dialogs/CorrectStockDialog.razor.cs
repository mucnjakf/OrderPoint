using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Shared.Extensions;
using OrderPoint.Admin.Stock.Api.Requests;
using OrderPoint.Admin.Stock.Dtos;

namespace OrderPoint.Admin.Stock.Dialogs;

// Starts with what the system thinks is there, in packs, so only the difference has to be typed
public sealed partial class CorrectStockDialog
{
    private const int PacksDecimals = 3;

    [Parameter]
    public StockItemDto StockItem { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private CorrectStockRequest Request { get; set; } = null!;

    private string AfterText => StockItem.GetQuantityText(Request.Quantity);

    protected override void OnInitialized()
    {
        Request = new CorrectStockRequest
        {
            PackSize = StockItem.PackSize,
            IsInPacks = true,
            EnteredQuantity = Math.Max(Math.Round(StockItem.Quantity / StockItem.PackSize, PacksDecimals), 0)
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