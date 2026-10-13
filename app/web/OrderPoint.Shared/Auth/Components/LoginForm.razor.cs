using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Shared.Auth.Api.Requests;
using OrderPoint.Shared.Auth.Enumerations;
using OrderPoint.Shared.Auth.Services;
using OrderPoint.Shared.Services;

namespace OrderPoint.Shared.Auth.Components;

public sealed partial class LoginForm
{
    [Inject]
    private AuthService AuthService { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public UserRole Role { get; set; }

    [Parameter]
    [EditorRequired]
    public string AppName { get; set; } = null!;

    private LoginRequest Request { get; } = new();

    // Set when the API asks for a new password: the form then shows the "choose your password" step
    private ChangeInitialPasswordRequest? InitialPasswordRequest { get; set; }

    private bool IsSigningIn { get; set; }

    private bool IsPasswordVisible { get; set; }

    private InputType PasswordInputType => IsPasswordVisible ? InputType.Text : InputType.Password;

    private string PasswordVisibilityIcon => IsPasswordVisible
        ? Icons.Material.Filled.VisibilityOff
        : Icons.Material.Filled.Visibility;

    protected override void OnInitialized()
    {
        Request.Role = Role;
    }

    private void TogglePasswordVisibility()
    {
        IsPasswordVisible = !IsPasswordVisible;
    }

    // On success the layout swaps this form for the requested page
    private async Task SignInAsync()
    {
        IsSigningIn = true;

        bool isPasswordChangeRequired = await ApiService.ExecuteAsync(() => AuthService.LoginAsync(Request));

        if (isPasswordChangeRequired)
        {
            InitialPasswordRequest = new ChangeInitialPasswordRequest
            {
                Email = Request.Email,
                CurrentPassword = Request.Password,
                Role = Request.Role
            };

            IsPasswordVisible = false;
        }

        IsSigningIn = false;
    }

    private async Task ChangeInitialPasswordAsync()
    {
        IsSigningIn = true;

        await ApiService.ExecuteAsync(
            () => AuthService.ChangeInitialPasswordAsync(InitialPasswordRequest!),
            $"Password set. Signed in as {Request.Email}");

        IsSigningIn = false;
    }

    private void BackToSignIn()
    {
        InitialPasswordRequest = null;
    }
}