using Microsoft.AspNetCore.Components;
using OrderPoint.Bar.Orders.Api;
using OrderPoint.Bar.Orders.Api.Requests;
using OrderPoint.Bar.Orders.Services;
using OrderPoint.Shared.Auth.Services;
using OrderPoint.Shared.Errors;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Enumerations;
using OrderPoint.Shared.Services;

namespace OrderPoint.Bar.Orders.Pages;

// A page rather than a dialog, because the reason for the guest is a form, which is cramped in a dialog on a phone
public sealed partial class DeclineOrderPage
{
    private const string AccountInactiveCode = "Auth.AccountInactive";

    private const string AccountDeletedCode = "Auth.AccountDeleted";

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private OrderApiClient OrderApiClient { get; set; } = null!;

    [Inject]
    private AuthService AuthService { get; set; } = null!;

    [Inject]
    private OrdersTabState OrdersTabState { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public Guid Id { get; set; }

    private OrderDto? Order { get; set; }

    private DeclineOrderRequest Request { get; } = new();

    private bool IsLoading { get; set; } = true;

    private bool IsDeclining { get; set; }

    // A tablet opened the order in the Orders page's panel, which shows it again; a phone has its own details page
    private string BackHref => OrdersTabState.SelectedOrderId == Id ? "/" : $"/orders/{Id}";

    // Any bartender can decline a pending order; an accepted one only its bartender
    private bool CanDecline =>
        Order?.Status is OrderStatus.Pending
        || (Order?.Status is OrderStatus.Accepted && Order.Bartender?.Id == AuthService.UserId);

    protected override async Task OnInitializedAsync()
    {
        await GetOrderAsync();
    }

    private async Task GetOrderAsync()
    {
        Order = await ApiService.ExecuteAsync(() => OrderApiClient.GetOrderAsync(Id));

        IsLoading = false;
    }

    private void BackToOrder()
    {
        NavigationManager.NavigateTo(BackHref);
    }

    // A refused decline (e.g. another bartender accepted first) reloads, so the ticket shows what really happened
    private async Task DeclineAsync()
    {
        int orderNumber = Order!.Number;

        IsDeclining = true;

        bool isSuccess = await ApiService.ExecuteAsync(
            EndSessionIfAccountClosedAsync,
            $"Order #{orderNumber} declined");

        IsDeclining = false;

        if (!AuthService.IsSignedIn)
        {
            return;
        }

        if (!isSuccess)
        {
            await GetOrderAsync();

            return;
        }

        if (OrdersTabState.SelectedOrderId == Id)
        {
            OrdersTabState.SelectedOrderId = null;
        }

        NavigationManager.NavigateTo("/");
    }

    // A bartender deactivated or deleted while signed in is refused by the API; the snackbar says why and the layout
    // shows the login form
    private async Task EndSessionIfAccountClosedAsync()
    {
        try
        {
            await OrderApiClient.DeclineOrderAsync(Id, Request);
        }
        catch (ApiException exception)
            when (exception.HasErrorCode(AccountInactiveCode) || exception.HasErrorCode(AccountDeletedCode))
        {
            await AuthService.EndSessionAsync();

            throw;
        }
    }
}