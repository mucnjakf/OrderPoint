namespace OrderPoint.Admin.Categories.Api.Requests;

internal sealed class ReorderCategoriesRequest
{
    public IReadOnlyList<Guid> Ids { get; set; } = [];
}