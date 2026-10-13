using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Shared.Auth.Api.Requests;
using OrderPoint.Shared.Auth.Enumerations;
using OrderPoint.Shared.Auth.Services;
using OrderPoint.Shared.Services;

namespace OrderPoint.Shared.Auth.Components;

// Split screen: a brand panel about the app on the left (desktop only) and the form on the right; on phones the
// panel shrinks to a small header above the form
public sealed partial class LoginForm
{
    // A faint tint and a border set the panel apart without a colour of its own (the theme is monochrome)
    private const string BrandPanelStyle =
        "flex: 1; background-color: rgba(var(--mud-palette-text-primary-rgb), 0.03); " +
        "border-right: 1px solid var(--mud-palette-lines-default);";

    private static readonly (string Icon, string Title, string Text)[] AdminFeatures =
    [
        (Icons.Material.Filled.Insights, "Live dashboard", "Revenue, busiest times and top items as they happen."),
        (Icons.Material.Filled.MenuBook, "Menu and staff", "Categories, items, prices and bartenders in one place."),
        (Icons.Material.Filled.Receipt, "Every order", "Follow each order from the table to served.")
    ];

    private static readonly (string Icon, string Title, string Text)[] BartenderFeatures =
    [
        (Icons.Material.Filled.Inbox, "Live queue", "New orders appear the moment guests place them."),
        (Icons.Material.Filled.LocalBar, "Your orders", "Accept, start and complete them with one tap."),
        (Icons.Material.Filled.Schedule, "Your shift", "What you served today, at a glance.")
    ];

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

    private string Tagline => Role == UserRole.Admin ? "Run your bar from one place." : "Take orders, make drinks.";

    private IReadOnlyList<(string Icon, string Title, string Text)> Features =>
        Role == UserRole.Admin ? AdminFeatures : BartenderFeatures;

    // Bartenders can't reset their own password; the admin does it for them
    private bool ShowsPasswordHint => Role == UserRole.Bartender;

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