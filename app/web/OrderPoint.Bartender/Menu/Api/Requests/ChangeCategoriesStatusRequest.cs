using OrderPoint.Bartender.Menu.Enumerations;

namespace OrderPoint.Bartender.Menu.Api.Requests;

internal sealed class ChangeCategoriesStatusRequest
{
    public IReadOnlyList<Guid> Ids { get; set; } = [];

    public CategoryStatus Status { get; set; }
}