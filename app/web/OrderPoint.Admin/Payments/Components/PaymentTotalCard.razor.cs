using Microsoft.AspNetCore.Components;

namespace OrderPoint.Admin.Payments.Components;

public sealed partial class PaymentTotalCard
{
    [Parameter]
    [EditorRequired]
    public string Icon { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public string Label { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public decimal Value { get; set; }

    [Parameter]
    [EditorRequired]
    public string Caption { get; set; } = null!;
}