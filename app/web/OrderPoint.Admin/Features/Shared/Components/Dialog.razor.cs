using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace OrderPoint.Admin.Features.Shared.Components;

public sealed partial class Dialog
{
    [Parameter]
    [EditorRequired]
    public string TitleIcon { get; set; }

    [Parameter]
    [EditorRequired]
    public Color TitleIconColor { get; set; }

    [Parameter]
    [EditorRequired]
    public string Title { get; set; }

    [Parameter]
    [EditorRequired]
    public string Content { get; set; }

    [Parameter]
    public string? AlertText { get; set; }

    [Parameter]
    [EditorRequired]
    public string ConfirmedButtonIcon { get; set; }

    [Parameter]
    [EditorRequired]
    public Color ConfirmButtonColor { get; set; }

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