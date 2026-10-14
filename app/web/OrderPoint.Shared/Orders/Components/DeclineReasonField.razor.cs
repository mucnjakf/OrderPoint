using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;

namespace OrderPoint.Shared.Orders.Components;

// A tap on a quick pick fills the field, which stays editable
public sealed partial class DeclineReasonField
{
    [Parameter]
    [EditorRequired]
    public string Reason { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<string> ReasonChanged { get; set; }

    [Parameter]
    [EditorRequired]
    public Expression<Func<string>> For { get; set; } = null!;

    private async Task SetReasonAsync(string reason)
    {
        Reason = reason;

        await ReasonChanged.InvokeAsync(reason);
    }
}