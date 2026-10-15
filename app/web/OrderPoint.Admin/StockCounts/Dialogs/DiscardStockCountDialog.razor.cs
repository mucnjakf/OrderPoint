using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Stock.Enumerations;
using OrderPoint.Admin.StockCounts.Dtos;

namespace OrderPoint.Admin.StockCounts.Dialogs;

public sealed partial class DiscardStockCountDialog
{
    private static readonly int AllGroupsCount = Enum.GetValues<StockGroup>().Length;

    [Parameter]
    public StockCountDto StockCount { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private void Confirm()
    {
        MudDialogInstance.Close(DialogResult.Ok(true));
    }

    private void Cancel()
    {
        MudDialogInstance.Cancel();
    }
}