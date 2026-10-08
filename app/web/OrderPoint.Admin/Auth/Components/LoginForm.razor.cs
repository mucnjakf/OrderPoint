using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Auth.Api.Requests;
using OrderPoint.Admin.Auth.Services;
using OrderPoint.Admin.Shared.Services;

namespace OrderPoint.Admin.Auth.Components;

public sealed partial class LoginForm
{
    [Inject]
    private AuthService AuthService { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    private LoginRequest Request { get; } = new();

    private bool IsSigningIn { get; set; }

    private bool IsPasswordVisible { get; set; }

    private InputType PasswordInputType => IsPasswordVisible ? InputType.Text : InputType.Password;

    private string PasswordVisibilityIcon => IsPasswordVisible
        ? Icons.Material.Filled.VisibilityOff
        : Icons.Material.Filled.Visibility;

    private void TogglePasswordVisibility()
    {
        IsPasswordVisible = !IsPasswordVisible;
    }

    private async Task SignInAsync()
    {
        IsSigningIn = true;

        // On success the layout swaps this form for the requested page
        await ApiService.ExecuteAsync(() => AuthService.LoginAsync(Request), $"Signed in as {Request.Email}");

        IsSigningIn = false;
    }
}