using Microsoft.AspNetCore.Components;
using OrderPoint.Shared.Orders.Dtos;

namespace OrderPoint.Bartender.Orders.Components;

// One tab's tickets, with skeletons while loading and an empty state
public sealed partial class OrderTicketList
{
    private const int LoadingTicketsCount = 3;

    [Parameter]
    [EditorRequired]
    public IReadOnlyList<OrderDto> Orders { get; set; } = [];

    [Parameter]
    [EditorRequired]
    public bool IsLoading { get; set; }

    [Parameter]
    [EditorRequired]
    public string EmptyStateIcon { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public string EmptyStateText { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public string EmptyStateDetail { get; set; } = null!;

    [Parameter]
    public Func<OrderDto, bool>? IsOrderNew { get; set; }

    [Parameter]
    public Guid? SelectedOrderId { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<OrderDto> OnOrderClick { get; set; }
}