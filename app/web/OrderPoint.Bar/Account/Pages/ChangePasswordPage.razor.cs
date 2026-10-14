using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Shared.Account.Api;
using OrderPoint.Shared.Account.Api.Requests;
using OrderPoint.Shared.Auth.Dtos;
using OrderPoint.Shared.Auth.Services;
using OrderPoint.Shared.Services;

namespace OrderPoint.Bar.Account.Pages;

// A page rather than the shared dialog, which is cramped on a phone; same fields and rules
public sealed partial class ChangePasswordPage
{
    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private AccountApiClient AccountApiClient { get; set; } = null!;

    [Inject]
    private AuthService AuthService { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private ChangePasswordRequest Request { get; } = new();

    private bool IsPasswordVisible { get; set; }

    private bool IsSaving { get; set; }

    private InputType PasswordInputType => IsPasswordVisible ? InputType.Text : InputType.Password;

    private string PasswordVisibilityIcon => IsPasswordVisible
        ? Icons.Material.Filled.VisibilityOff
        : Icons.Material.Filled.Visibility;

    private void TogglePasswordVisibility()
    {
        IsPasswordVisible = !IsPasswordVisible;
    }

    private async Task ChangePasswordAsync()
    {
        IsSaving = true;

        bool isSuccess = await ApiService.ExecuteAsync(ReplacePasswordAsync, "Password changed successfully");

        IsSaving = false;

        if (!isSuccess)
        {
            return;
        }

        NavigationManager.NavigateTo("/account");
    }

    private async Task ReplacePasswordAsync()
    {
        AuthTokensDto tokens = await AccountApiClient.ChangePasswordAsync(Request);

        await AuthService.ReplaceTokensAsync(tokens);
    }
}