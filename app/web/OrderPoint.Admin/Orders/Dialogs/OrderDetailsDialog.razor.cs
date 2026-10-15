using System.Globalization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Orders.Api;
using OrderPoint.Admin.Orders.Api.Requests;
using OrderPoint.Shared.Extensions;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Enumerations;
using OrderPoint.Shared.Orders.Services;
using OrderPoint.Shared.Services;

namespace OrderPoint.Admin.Orders.Dialogs;

public sealed partial class OrderDetailsDialog : IDisposable
{
    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private OrderApiClient OrderApiClient { get; set; } = null!;

    [Inject]
    private OrdersHubService OrdersHubService { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Parameter]
    public OrderDto Order { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    // The parameter is reset whenever the dialog provider re-renders, so live updates go here
    private OrderDto CurrentOrder { get; set; } = null!;

    private bool IsRefund => CurrentOrder.PaymentStatus is PaymentStatus.RefundDue or PaymentStatus.Refunded;

    // Equal tiles: one row, or two rows of three for a refund
    private string PaymentFactsStyle =>
        $"display: grid; grid-template-columns: repeat({(IsRefund ? 3 : PaymentFacts.Count)}, minmax(0, 1fr)); " +
        "gap: 12px;";

    private IReadOnlyList<PaymentFact> PaymentFacts
    {
        get
        {
            string total = CurrentOrder.Total.ToString("C", CultureInfo.CurrentCulture);

            if (CurrentOrder.PaymentStatus == PaymentStatus.Unpaid)
            {
                return CurrentOrder.Status switch
                {
                    OrderStatus.Declined =>
                    [
                        new PaymentFact(Icons.Material.Filled.Euro, "Amount due", "Nothing"),
                        new PaymentFact(Icons.Material.Filled.Info, "Why", "Declined before it was paid")
                    ],
                    OrderStatus.Cancelled =>
                    [
                        new PaymentFact(Icons.Material.Filled.Euro, "Amount due", "Nothing"),
                        new PaymentFact(Icons.Material.Filled.Info, "Why", "Cancelled before it was paid")
                    ],
                    _ =>
                    [
                        new PaymentFact(Icons.Material.Filled.Euro, "Amount due", total),
                        new PaymentFact(Icons.Material.Filled.PointOfSale, "Pays at", "The register")
                    ]
                };
            }

            // A guest's card in the customer app has nobody at the register
            string paidTo = CurrentOrder.PaidByBartender is { } paidBy
                ? $"{paidBy.FirstName} {paidBy.LastName}"
                : "Guest, online";

            string paidAt = CurrentOrder.PaidAtUtc!.Value.ToDisplayDateTime(TimeZoneService.TimeZone);

            List<PaymentFact> facts =
            [
                new PaymentFact(Icons.Material.Filled.Euro, "Amount", total),
                new PaymentFact(Icons.Material.Filled.Schedule, "Paid", paidAt),
                new PaymentFact(Icons.Material.Filled.Person, "Paid to", paidTo, CurrentOrder.PaidByBartender)
            ];

            if (CurrentOrder.CashReceived is { } cashReceived)
            {
                string received = cashReceived.ToString("C", CultureInfo.CurrentCulture);
                string change = (cashReceived - CurrentOrder.Total).ToString("C", CultureInfo.CurrentCulture);

                facts.Add(new PaymentFact(Icons.Material.Filled.Payments, "Cash received", received));
                facts.Add(new PaymentFact(Icons.Material.Filled.CurrencyExchange, "Change", change));
            }
            else if (IsRefund)
            {
                bool isOnline = CurrentOrder.PaidByBartender is null;
                string method = isOnline ? "Card in the customer app" : "Card on the terminal";
                string refundTo = isOnline ? "The guest's card" : "The card, on the terminal";

                facts.Add(new PaymentFact(Icons.Material.Filled.CreditCard, "Method", method));
                facts.Add(new PaymentFact(Icons.Material.Filled.Undo, "Refund to", refundTo));
            }

            if (CurrentOrder.PaymentStatus == PaymentStatus.RefundDue)
            {
                facts.Add(new PaymentFact(Icons.Material.Filled.AssignmentReturn, "Refund", "Due at the register"));
            }

            if (CurrentOrder.RefundedAtUtc is { } refundedAtUtc)
            {
                string refundedAt = refundedAtUtc.ToDisplayDateTime(TimeZoneService.TimeZone);

                facts.Add(new PaymentFact(Icons.Material.Filled.AssignmentReturn, "Refunded", refundedAt));
            }

            return facts;
        }
    }

    private bool IsDeclining { get; set; }

    // Pending, accepted or active: the admin can still decline it
    private bool IsOpen => CurrentOrder.Status is OrderStatus.Pending or OrderStatus.Accepted or OrderStatus.Active;

    private IReadOnlyList<(OrderStatus Status, DateTimeOffset? ReachedAtUtc)> TimelineSteps =>
        CurrentOrder.Status switch
        {
            OrderStatus.Declined =>
            [
                (OrderStatus.Pending, CurrentOrder.CreatedAtUtc),
                (OrderStatus.Declined, CurrentOrder.DeclinedAtUtc)
            ],
            OrderStatus.Cancelled =>
            [
                (OrderStatus.Pending, CurrentOrder.CreatedAtUtc),
                (OrderStatus.Cancelled, CurrentOrder.CancelledAtUtc)
            ],
            _ =>
            [
                (OrderStatus.Pending, CurrentOrder.CreatedAtUtc),
                (OrderStatus.Accepted, CurrentOrder.AcceptedAtUtc),
                (OrderStatus.Active, CurrentOrder.ActivatedAtUtc),
                (OrderStatus.Completed, CurrentOrder.CompletedAtUtc)
            ]
        };

    protected override void OnInitialized()
    {
        CurrentOrder = Order;

        OrdersHubService.OrdersChanged += OnOrdersChanged;
    }

    public void Dispose()
    {
        OrdersHubService.OrdersChanged -= OnOrdersChanged;
    }

    private void OnOrdersChanged()
    {
        _ = InvokeAsync(GetOrderAsync);
    }

    private async Task GetOrderAsync()
    {
        OrderDto? order = await ApiService.ExecuteAsync(() => OrderApiClient.GetOrderAsync(Order.Id));

        if (order is null)
        {
            return;
        }

        CurrentOrder = order;

        StateHasChanged();
    }

    private async Task ShowDeclineOrderDialogAsync()
    {
        var parameters = new DialogParameters<DeclineOrderDialog>
        {
            { dialog => dialog.Order, CurrentOrder }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<DeclineOrderDialog>(string.Empty, parameters, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        var request = (dialogResult.Data as DeclineOrderRequest)!;

        IsDeclining = true;

        bool isSuccess = await ApiService.ExecuteAsync(
            () => OrderApiClient.DeclineOrderAsync(CurrentOrder.Id, request),
            $"Order #{CurrentOrder.Number} declined");

        IsDeclining = false;

        if (isSuccess)
        {
            await GetOrderAsync();
        }
    }

    private static decimal GetSubtotal(OrderItemDto orderItem)
    {
        return orderItem.Quantity * orderItem.UnitPrice;
    }

    private void Close()
    {
        MudDialogInstance.Close(DialogResult.Ok(true));
    }

    // One tile of the payment; with a bartender it shows their avatar before the value
    private sealed record PaymentFact(string Icon, string Label, string Value, OrderBartenderDto? Bartender = null);
}