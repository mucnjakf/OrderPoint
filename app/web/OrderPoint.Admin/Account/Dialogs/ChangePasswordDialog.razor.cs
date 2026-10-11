using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Account.Api.Requests;

namespace OrderPoint.Admin.Account.Dialogs;

public sealed partial class ChangePasswordDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private ChangePasswordRequest Request { get; } = new();

    private bool IsPasswordVisible { get; set; }

    private InputType PasswordInputType => IsPasswordVisible ? InputType.Text : InputType.Password;

    private string PasswordVisibilityIcon => IsPasswordVisible
        ? Icons.Material.Filled.VisibilityOff
        : Icons.Material.Filled.Visibility;

    private void TogglePasswordVisibility()
    {
        IsPasswordVisible = !IsPasswordVisible;
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