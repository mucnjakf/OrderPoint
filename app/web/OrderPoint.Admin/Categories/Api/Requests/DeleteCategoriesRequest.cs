namespace OrderPoint.Admin.Categories.Api.Requests;

internal sealed class DeleteCategoriesRequest
{
    public IReadOnlyList<Guid> Ids { get; set; } = [];
}