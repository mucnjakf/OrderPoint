using Microsoft.AspNetCore.Components;

namespace OrderPoint.Bartender.Orders.Pages;

// Phones open an order on its own page; tablets show it next to the list (OrdersPage)
public sealed partial class OrderDetailsPage
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public Guid Id { get; set; }

    private void BackToOrders()
    {
        NavigationManager.NavigateTo("/");
    }
}