using OrderPoint.Admin.Categories.Enumerations;

namespace OrderPoint.Admin.Categories.Api.Requests;

internal sealed class ChangeCategoriesStatusRequest
{
    public IReadOnlyList<Guid> Ids { get; set; } = [];

    public CategoryStatus Status { get; set; }
}