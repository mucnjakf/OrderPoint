using OrderPoint.Admin.Bartenders.Enumerations;

namespace OrderPoint.Admin.Bartenders.Api.Requests;

internal sealed class ChangeBartendersStatusRequest
{
    public IReadOnlyList<Guid> Ids { get; set; } = [];

    public BartenderStatus Status { get; set; }
}