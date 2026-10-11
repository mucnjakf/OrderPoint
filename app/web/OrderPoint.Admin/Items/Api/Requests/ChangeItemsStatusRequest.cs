using OrderPoint.Admin.Items.Enumerations;

namespace OrderPoint.Admin.Items.Api.Requests;

internal sealed class ChangeItemsStatusRequest
{
    public IReadOnlyList<Guid> Ids { get; set; } = [];

    public ItemStatus Status { get; set; }
}