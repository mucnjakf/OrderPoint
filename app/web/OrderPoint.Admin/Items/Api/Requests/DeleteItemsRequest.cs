namespace OrderPoint.Admin.Items.Api.Requests;

internal sealed class DeleteItemsRequest
{
    public IReadOnlyList<Guid> Ids { get; set; } = [];
}