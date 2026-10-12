using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Bartenders.Dtos;
using OrderPoint.Admin.Bartenders.Enumerations;
using OrderPoint.Shared.Extensions;

namespace OrderPoint.Admin.Bartenders.Dialogs;

public sealed partial class ChangeBartendersStatusDialog
{
    [Parameter]
    public IReadOnlyList<BartenderDto> Bartenders { get; set; } = [];

    [Parameter]
    public BartenderStatus Status { get; set; }

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private bool IsActivating => Status is BartenderStatus.Active;

    private string Title => IsActivating ? "Activate bartenders" : "Deactivate bartenders";

    private string StatusIcon => IsActivating ? Icons.Material.Filled.CheckCircle : Icons.Material.Filled.Block;

    private Color StatusColor => IsActivating ? Color.Success : Color.Warning;

    private Severity AlertSeverity => IsActivating ? Severity.Info : Severity.Warning;

    private string AlertText => IsActivating
        ? "They can sign in to the bartender app again."
        : "They are signed out everywhere and can't sign in until they are activated again.";

    private string ConfirmText =>
        $"{(IsActivating ? "Activate" : "Deactivate")} {Bartenders.Count.ToCountText("bartender", "bartenders")}";

    private void Confirm()
    {
        MudDialogInstance.Close(DialogResult.Ok(true));
    }

    private void Cancel()
    {
        MudDialogInstance.Cancel();
    }
}