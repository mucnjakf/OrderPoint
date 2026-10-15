using Microsoft.AspNetCore.Components;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Enumerations;
using OrderPoint.Shared.Services;

namespace OrderPoint.Register.Orders.Components;

// What the register needs to know about an order before taking or giving back money
public sealed partial class OrderCardDetails
{
    private const string SecondaryStyle = "color: var(--mud-palette-text-secondary)";

    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public OrderDto Order { get; set; } = null!;

    private int ItemsCount => Order.Items.Sum(orderItem => orderItem.Quantity);

    private string SourceText => Order.Source switch
    {
        OrderSource.Bartender => "Taken in the Bartender app",
        OrderSource.Register => "Sold at the register",
        _ => "Ordered by the guest"
    };
}