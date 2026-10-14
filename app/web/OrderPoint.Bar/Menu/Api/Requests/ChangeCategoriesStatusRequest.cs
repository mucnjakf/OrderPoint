using OrderPoint.Bar.Menu.Enumerations;

namespace OrderPoint.Bar.Menu.Api.Requests;

internal sealed class ChangeCategoriesStatusRequest
{
    public IReadOnlyList<Guid> Ids { get; set; } = [];

    public CategoryStatus Status { get; set; }
}