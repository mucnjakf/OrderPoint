using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OrderPoint.Admin.Stock.Api.Requests;

// Entered in packs or in the base unit; the API takes base units
internal sealed class CorrectStockRequest
{
    [JsonIgnore]
    [Range(0, 1_000_000, ErrorMessage = "Quantity must not be negative.")]
    public decimal EnteredQuantity { get; set; }

    [JsonIgnore]
    public bool IsInPacks { get; set; }

    [JsonIgnore]
    public decimal PackSize { get; set; }

    [StringLength(200, ErrorMessage = "Note must be at most 200 characters.")]
    public string? Note { get; set; }

    public decimal Quantity => IsInPacks ? EnteredQuantity * PackSize : EnteredQuantity;
}