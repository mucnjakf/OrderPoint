using System.Globalization;
using Microsoft.AspNetCore.Components;
using OrderPoint.Bartender.Orders.Api;
using OrderPoint.Bartender.Orders.Enumerations;
using OrderPoint.Bartender.Shared.Layout;
using OrderPoint.Shared.Auth.Services;
using OrderPoint.Shared.Errors;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Enumerations;
using OrderPoint.Shared.Orders.Services;
using OrderPoint.Shared.Services;

namespace OrderPoint.Bartender.Orders.Components;

// Loads the order itself and follows it live; the buttons match what this bartender may do next
public sealed partial class OrderDetails : IDisposable
{
    private const string AccountInactiveCode = "Auth.AccountInactive";

    private const string AccountDeletedCode = "Auth.AccountDeleted";

    private const string NotReachedStyle = "color: var(--mud-palette-text-disabled)";

    private const string SpinnerStyle = "height: 20px; width: 20px;";

    // Phone: the buttons are fixed to the bottom of the screen, above the bottom navigation, and the details leave
    // room for them
    private const string PageActionBarStyle =
        $"position: fixed; left: 0; right: 0; bottom: {BottomNavigation.Height}; " +
        "z-index: var(--mud-zindex-appbar); border-radius: 0; border-top: 1px solid var(--mud-palette-lines-default);";

    private const string PageContentStyle = "padding-bottom: 84px;";

    // Tablet panel: the details scroll inside the panel and the buttons close it off at the bottom
    private const string PanelRootStyle = "display: flex; flex-direction: column; height: 100%;";

    private const string PanelContentStyle = "flex: 1; min-height: 0; overflow-y: auto;";

    private const string PanelActionBarStyle = "margin-top: 12px;";

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private OrderApiClient OrderApiClient { get; set; } = null!;

    [Inject]
    private AuthService AuthService { get; set; } = null!;

    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Inject]
    private OrdersHubService OrdersHubService { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public Guid OrderId { get; set; }

    [Parameter]
    public bool IsInPanel { get; set; }

    // Completed: nothing left to do here
    [Parameter]
    public EventCallback OnFinished { get; set; }

    // Shows a close button (the tablet panel; the phone page has its back arrow)
    [Parameter]
    public EventCallback OnClose { get; set; }

    private OrderDto? Order { get; set; }

    private bool IsLoading { get; set; } = true;

    // The button waiting for the API shows a spinner, and every button is disabled meanwhile
    private OrderAction? WorkingAction { get; set; }

    private string? RootStyle => IsInPanel ? PanelRootStyle : null;

    private string? ContentStyle => IsInPanel ? PanelContentStyle : HasActions ? PageContentStyle : null;

    private string ActionBarStyle => IsInPanel ? PanelActionBarStyle : PageActionBarStyle;

    // The phone bar is flat with a top border; the panel bar is a card like the details
    private int ActionBarElevation => IsInPanel ? 1 : 0;

    private bool IsMine => Order?.Bartender is not null && Order.Bartender.Id == AuthService.UserId;

    private bool CanAccept => Order?.Status is OrderStatus.Pending;

    // Any bartender can decline a pending order; an accepted one only its bartender
    private bool CanDecline =>
        Order?.Status is OrderStatus.Pending || (Order?.Status is OrderStatus.Accepted && IsMine);

    private bool CanStart => Order?.Status is OrderStatus.Accepted && IsMine;

    private bool CanComplete => Order?.Status is OrderStatus.Active && IsMine;

    private bool HasActions => CanAccept || CanDecline || CanStart || CanComplete;

    private string TableMapHref => $"/tables?table={Uri.EscapeDataString(Order!.TableCode)}";

    private string BartenderInitials => Order?.Bartender is null
        ? string.Empty
        : $"{Order.Bartender.FirstName[0]}{Order.Bartender.LastName[0]}";

    private string BartenderText => IsMine
        ? "Handled by you"
        : $"Handled by {Order!.Bartender!.FirstName} {Order.Bartender.LastName}";

    private IReadOnlyList<(OrderStatus Status, DateTimeOffset? ReachedAtUtc)> StatusSteps =>
        Order!.Status switch
        {
            OrderStatus.Declined =>
            [
                (OrderStatus.Pending, Order.CreatedAtUtc),
                (OrderStatus.Declined, Order.DeclinedAtUtc)
            ],
            OrderStatus.Cancelled =>
            [
                (OrderStatus.Pending, Order.CreatedAtUtc),
                (OrderStatus.Cancelled, Order.CancelledAtUtc)
            ],
            _ =>
            [
                (OrderStatus.Pending, Order.CreatedAtUtc),
                (OrderStatus.Accepted, Order.AcceptedAtUtc),
                (OrderStatus.Active, Order.ActivatedAtUtc),
                (OrderStatus.Completed, Order.CompletedAtUtc)
            ]
        };

    protected override async Task OnInitializedAsync()
    {
        OrdersHubService.OrdersChanged += OnOrdersChanged;

        await GetOrderAsync();
    }

    private static string GetLineTotalText(OrderItemDto orderItem)
    {
        return (orderItem.Quantity * orderItem.UnitPrice).ToString("C", CultureInfo.CurrentCulture);
    }

    public void Dispose()
    {
        OrdersHubService.OrdersChanged -= OnOrdersChanged;
    }

    private async Task CloseAsync()
    {
        await OnClose.InvokeAsync();
    }

    private void OnOrdersChanged()
    {
        _ = InvokeAsync(GetOrderAsync);
    }

    // A failed reload keeps what is shown; only a failed first load means the order can't be shown
    private async Task GetOrderAsync()
    {
        OrderDto? order = await ApiService.ExecuteAsync(() => OrderApiClient.GetOrderAsync(OrderId));

        if (order is not null || IsLoading)
        {
            Order = order;
        }

        IsLoading = false;

        StateHasChanged();
    }

    // The spinner takes the icon's place while that action waits for the API
    private string? GetStartIcon(OrderAction action, string icon)
    {
        return WorkingAction == action ? null : icon;
    }

    private async Task AcceptAsync()
    {
        await ChangeStatusAsync(
            OrderAction.Accept,
            () => OrderApiClient.AcceptOrderAsync(OrderId),
            $"Order #{Order!.Number} accepted",
            isFinishing: false);
    }

    private async Task StartAsync()
    {
        await ChangeStatusAsync(
            OrderAction.Start,
            () => OrderApiClient.ActivateOrderAsync(OrderId),
            $"Order #{Order!.Number} started",
            isFinishing: false);
    }

    private async Task CompleteAsync()
    {
        await ChangeStatusAsync(
            OrderAction.Complete,
            () => OrderApiClient.CompleteOrderAsync(OrderId),
            $"Order #{Order!.Number} completed",
            isFinishing: true);
    }

    // Declining needs a reason, so it has its own page
    private void OpenDeclinePage()
    {
        NavigationManager.NavigateTo($"/orders/{OrderId}/decline");
    }

    // A refused change (e.g. another bartender accepted first) reloads, so the screen shows what really happened
    private async Task ChangeStatusAsync(
        OrderAction action,
        Func<Task> changeStatus,
        string successMessage,
        bool isFinishing)
    {
        WorkingAction = action;

        bool isSuccess = await ApiService.ExecuteAsync(
            () => EndSessionIfAccountClosedAsync(changeStatus),
            successMessage);

        WorkingAction = null;

        if (!AuthService.IsSignedIn)
        {
            return;
        }

        if (isSuccess && isFinishing)
        {
            await OnFinished.InvokeAsync();

            return;
        }

        await GetOrderAsync();
    }

    // A bartender deactivated or deleted while signed in is refused by the API; the snackbar says why and the layout
    // shows the login form
    private async Task EndSessionIfAccountClosedAsync(Func<Task> changeStatus)
    {
        try
        {
            await changeStatus();
        }
        catch (ApiException exception)
            when (exception.HasErrorCode(AccountInactiveCode) || exception.HasErrorCode(AccountDeletedCode))
        {
            await AuthService.EndSessionAsync();

            throw;
        }
    }
}