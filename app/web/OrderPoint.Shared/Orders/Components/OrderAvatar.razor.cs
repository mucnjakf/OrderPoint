using Microsoft.AspNetCore.Components;

namespace OrderPoint.Shared.Orders.Components;

public sealed partial class OrderAvatar
{
    [Parameter]
    [EditorRequired]
    public int Number { get; set; }
}