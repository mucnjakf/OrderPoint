using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Account.Api;
using OrderPoint.Admin.Account.Api.Requests;
using OrderPoint.Admin.Account.Dialogs;
using OrderPoint.Admin.Account.Services;
using OrderPoint.Admin.Auth.Dtos;
using OrderPoint.Admin.Auth.Services;
using OrderPoint.Admin.Shared.Dtos;
using OrderPoint.Admin.Shared.Services;

namespace OrderPoint.Admin.Account.Pages;

public sealed partial class AccountPage : IDisposable
{
    private const string CurrentImageFileName = "Current image";

    [Inject]
    private AuthService AuthService { get; set; } = null!;

    [Inject]
    private AccountService AccountService { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private AccountApiClient AccountApiClient { get; set; } = null!;

    private List<BreadcrumbItem> Breadcrumbs { get; set; } =
    [
        new("Dashboard", href: "/", icon: Icons.Material.Filled.Dashboard),
        new("Account", href: null, disabled: true, icon: Icons.Material.Filled.AccountCircle)
    ];

    private string? ImageFileName => AccountService.ImageUrl is null ? null : CurrentImageFileName;

    private string Initial => string.IsNullOrEmpty(AuthService.Email)
        ? string.Empty
        : char.ToUpperInvariant(AuthService.Email[0]).ToString();

    protected override void OnInitialized()
    {
        AccountService.Changed += OnAccountChanged;
    }

    public void Dispose()
    {
        AccountService.Changed -= OnAccountChanged;
    }

    private void OnAccountChanged()
    {
        _ = InvokeAsync(StateHasChanged);
    }

    private async Task OnImageSelectedAsync(ImageFileDto image)
    {
        bool isSuccess = await ApiService.ExecuteAsync(
            () => AccountApiClient.UpdateAccountImageAsync(image),
            "Image updated successfully");

        if (isSuccess)
        {
            await AccountService.LoadAsync();
        }
    }

    private async Task OnImageRemovedAsync()
    {
        bool isSuccess = await ApiService.ExecuteAsync(
            () => AccountApiClient.DeleteAccountImageAsync(),
            "Image removed successfully");

        if (isSuccess)
        {
            await AccountService.LoadAsync();
        }
    }

    private async Task ShowChangePasswordDialogAsync()
    {
        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<ChangePasswordDialog>(string.Empty, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        var request = (dialogResult.Data as ChangePasswordRequest)!;

        await ApiService.ExecuteAsync(
            () => ChangePasswordAsync(request),
            "Password changed successfully");
    }

    private async Task ChangePasswordAsync(ChangePasswordRequest request)
    {
        AuthTokensDto tokens = await AccountApiClient.ChangePasswordAsync(request);

        await AuthService.ReplaceTokensAsync(tokens);
    }

    private async Task ShowSignOutDialogAsync()
    {
        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.ExtraSmall,
            FullWidth = true
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<SignOutDialog>(string.Empty, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        await ApiService.ExecuteAsync(() => AuthService.LogoutAsync(), "Signed out");
    }

    private async Task ShowSignOutEverywhereDialogAsync()
    {
        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<SignOutEverywhereDialog>(string.Empty, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        await ApiService.ExecuteAsync(
            () => SignOutEverywhereAsync(),
            "Signed out on every device");
    }

    private async Task SignOutEverywhereAsync()
    {
        await AccountApiClient.SignOutEverywhereAsync();

        await AuthService.EndSessionAsync();
    }
}