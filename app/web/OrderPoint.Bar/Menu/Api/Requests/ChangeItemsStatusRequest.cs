using OrderPoint.Bar.Menu.Enumerations;

namespace OrderPoint.Bar.Menu.Api.Requests;

internal sealed class ChangeItemsStatusRequest
{
    public IReadOnlyList<Guid> Ids { get; set; } = [];

    public ItemStatus Status { get; set; }
}