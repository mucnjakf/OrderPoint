using Microsoft.AspNetCore.Components;
using OrderPoint.Shared.Tables.Dtos;
using OrderPoint.Shared.Tables.Enumerations;

namespace OrderPoint.Customer.Tables.Components;

// A button to ask, or, while the request is open, that a bartender is on the way
public sealed partial class TableRequestCard
{
    [Parameter]
    [EditorRequired]
    public TableRequestType Type { get; set; }

    [Parameter]
    [EditorRequired]
    public TableRequestDto? OpenRequest { get; set; }

    [Parameter]
    [EditorRequired]
    public bool IsSending { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<TableRequestType> OnRequest { get; set; }

    private string WaitingText => Type is TableRequestType.Bill
        ? "The bill is on its way"
        : "A bartender is on the way";

    private async Task OnRequestClickAsync()
    {
        await OnRequest.InvokeAsync(Type);
    }
}