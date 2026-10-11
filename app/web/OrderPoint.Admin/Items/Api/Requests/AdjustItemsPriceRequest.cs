using OrderPoint.Admin.Items.Enumerations;

namespace OrderPoint.Admin.Items.Api.Requests;

internal sealed class AdjustItemsPriceRequest
{
    public IReadOnlyList<Guid> Ids { get; set; } = [];

    public PriceAdjustmentMode Mode { get; set; }

    public decimal Value { get; set; }
}