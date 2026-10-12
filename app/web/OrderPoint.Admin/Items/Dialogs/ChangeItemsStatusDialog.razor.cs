using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Items.Dtos;
using OrderPoint.Admin.Items.Enumerations;
using OrderPoint.Shared.Extensions;

namespace OrderPoint.Admin.Items.Dialogs;

public sealed partial class ChangeItemsStatusDialog
{
    [Parameter]
    public IReadOnlyList<ItemDto> Items { get; set; } = [];

    [Parameter]
    public ItemStatus Status { get; set; }

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private bool IsActivating => Status is ItemStatus.Active;

    private string Title => IsActivating ? "Activate items" : "Deactivate items";

    private string StatusIcon => IsActivating ? Icons.Material.Filled.CheckCircle : Icons.Material.Filled.Block;

    private Color StatusColor => IsActivating ? Color.Success : Color.Warning;

    private Severity AlertSeverity => IsActivating ? Severity.Info : Severity.Warning;

    private string AlertText => IsActivating
        ? "These items can be ordered again, unless their category is inactive."
        : "These items can't be ordered until they are activated again.";

    private string ConfirmText =>
        $"{(IsActivating ? "Activate" : "Deactivate")} {Items.Count.ToCountText("item", "items")}";

    private void Confirm()
    {
        MudDialogInstance.Close(DialogResult.Ok(true));
    }

    private void Cancel()
    {
        MudDialogInstance.Cancel();
    }
}