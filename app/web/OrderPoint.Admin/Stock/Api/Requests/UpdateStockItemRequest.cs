using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using OrderPoint.Admin.Stock.Enumerations;
using OrderPoint.Admin.Suppliers.Dtos;

namespace OrderPoint.Admin.Stock.Api.Requests;

// The dialog asks for the par level in packs; the API takes base units
internal sealed class UpdateStockItemRequest
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, ErrorMessage = "Name must be at most 100 characters.")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Group is required.")]
    public StockGroup Group { get; set; }

    [Required(ErrorMessage = "Pack name is required.")]
    [StringLength(30, ErrorMessage = "Pack name must be at most 30 characters.")]
    public string PackName { get; set; } = null!;

    [Range(0.001, 1_000_000, ErrorMessage = "Pack size must be between 0.001 and 1,000,000.")]
    public decimal PackSize { get; set; }

    [Range(0, 100_000, ErrorMessage = "Cost must be between 0 and 100,000.")]
    public decimal PackCost { get; set; }

    [JsonIgnore]
    [Range(0, 100_000, ErrorMessage = "Par level must be between 0 and 100,000 packs.")]
    public decimal ParLevelInPacks { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    public StockItemStatus Status { get; set; }

    // Optional: who it is usually bought from
    [JsonIgnore]
    public SupplierOptionDto? Supplier { get; set; }

    public Guid? SupplierId => Supplier?.Id;

    public decimal ParLevel => ParLevelInPacks * PackSize;
}