using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Shared.Extensions;
using OrderPoint.Admin.StockCounts.Api.Requests;
using OrderPoint.Admin.StockCounts.Dtos;

namespace OrderPoint.Admin.StockCounts.Dialogs;

// Sums up what closing will do; the page closes the count with the note
public sealed partial class CloseStockCountDialog
{
    private const string FormId = "close-stock-count-form";

    private const string FactsStyle = "display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 12px;";

    private const int BiggestLossesCount = 3;

    [Parameter]
    public StockCountDto StockCount { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private CloseStockCountRequest Request { get; } = new();

    private int NotCountedCount => StockCount.LinesCount - StockCount.CountedCount;

    private IReadOnlyList<StockCountLineDto> BiggestLosses => StockCount.Lines
        .Where(line => line.ValueChange < 0)
        .OrderBy(line => line.ValueChange)
        .Take(BiggestLossesCount)
        .ToList();

    private IReadOnlyList<(string Label, string Value, string? Style)> Facts =>
    [
        ("Counted", $"{StockCount.CountedCount} of {StockCount.LinesCount}", null),
        ("Changed", StockCount.Lines.Count(line => line.QuantityChange is not null and not 0).ToString(), null),
        ("Difference", StockCount.ValueChange.ToSignedMoneyText(), StockCount.ValueChange.GetValueChangeStyle())
    ];

    private void Submit()
    {
        MudDialogInstance.Close(DialogResult.Ok(Request));
    }

    private void Cancel()
    {
        MudDialogInstance.Cancel();
    }
}