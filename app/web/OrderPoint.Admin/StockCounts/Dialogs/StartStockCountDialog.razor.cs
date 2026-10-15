using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Stock.Enumerations;
using OrderPoint.Admin.StockCounts.Api.Requests;

namespace OrderPoint.Admin.StockCounts.Dialogs;

public sealed partial class StartStockCountDialog
{
    private const string GroupsStyle = "display: grid; grid-template-columns: repeat(2, minmax(0, 1fr));";

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    // Every group by default: a full count
    private HashSet<StockGroup> SelectedGroups { get; } = [.. Enum.GetValues<StockGroup>()];

    private void ToggleGroup(StockGroup group, bool isChecked)
    {
        if (isChecked)
        {
            SelectedGroups.Add(group);
        }
        else
        {
            SelectedGroups.Remove(group);
        }
    }

    private void Confirm()
    {
        MudDialogInstance.Close(DialogResult.Ok(new StartStockCountRequest(SelectedGroups.Order().ToList())));
    }

    private void Cancel()
    {
        MudDialogInstance.Cancel();
    }
}