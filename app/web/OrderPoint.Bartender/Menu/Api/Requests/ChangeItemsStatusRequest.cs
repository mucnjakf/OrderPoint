using OrderPoint.Bartender.Menu.Enumerations;

namespace OrderPoint.Bartender.Menu.Api.Requests;

internal sealed class ChangeItemsStatusRequest
{
    public IReadOnlyList<Guid> Ids { get; set; } = [];

    public ItemStatus Status { get; set; }
}