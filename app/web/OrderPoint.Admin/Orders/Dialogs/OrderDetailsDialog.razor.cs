using System.Globalization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Shared.Extensions;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Admin.Orders.Api;
using OrderPoint.Admin.Orders.Api.Requests;
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

    // One row of equal tiles, however many facts the payment has
    private string PaymentFactsStyle =>
        $"display: grid; grid-template-columns: repeat({PaymentFacts.Count}, minmax(0, 1fr)); gap: 12px;";

    private IReadOnlyList<(string Icon, string Label, string Value)> PaymentFacts
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
                        (Icons.Material.Filled.Euro, "Amount due", "Nothing"),
                        (Icons.Material.Filled.Info, "Why", "Declined before it was paid")
                    ],
                    OrderStatus.Cancelled =>
                    [
                        (Icons.Material.Filled.Euro, "Amount due", "Nothing"),
                        (Icons.Material.Filled.Info, "Why", "Cancelled before it was paid")
                    ],
                    _ =>
                    [
                        (Icons.Material.Filled.Euro, "Amount due", total),
                        (Icons.Material.Filled.PointOfSale, "Pays at", "The register")
                    ]
                };
            }

            // A guest's card in the customer app has nobody at the register
            string paidTo = CurrentOrder.PaidByBartender is { } paidBy
                ? $"{paidBy.FirstName} {paidBy.LastName}"
                : "Guest, online";

            string paidAt = CurrentOrder.PaidAtUtc!.Value.ToDisplayDateTime(TimeZoneService.TimeZone);

            List<(string Icon, string Label, string Value)> facts =
            [
                (Icons.Material.Filled.Euro, "Amount", total),
                (Icons.Material.Filled.Schedule, "Paid", paidAt),
                (Icons.Material.Filled.Person, "Paid to", paidTo)
            ];

            if (CurrentOrder.CashReceived is { } cashReceived)
            {
                string received = cashReceived.ToString("C", CultureInfo.CurrentCulture);
                string change = (cashReceived - CurrentOrder.Total).ToString("C", CultureInfo.CurrentCulture);

                facts.Add((Icons.Material.Filled.Payments, "Cash received", received));
                facts.Add((Icons.Material.Filled.CurrencyExchange, "Change", change));
            }

            if (CurrentOrder.PaymentStatus == PaymentStatus.RefundDue)
            {
                facts.Add((Icons.Material.Filled.AssignmentReturn, "Refund", "Due at the register"));
            }

            if (CurrentOrder.RefundedAtUtc is { } refundedAtUtc)
            {
                string refundedAt = refundedAtUtc.ToDisplayDateTime(TimeZoneService.TimeZone);

                facts.Add((Icons.Material.Filled.AssignmentReturn, "Refunded", refundedAt));
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
}