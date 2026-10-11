namespace OrderPoint.Admin.Bartenders.Api.Requests;

internal sealed class DeleteBartendersRequest
{
    public IReadOnlyList<Guid> Ids { get; set; } = [];
}