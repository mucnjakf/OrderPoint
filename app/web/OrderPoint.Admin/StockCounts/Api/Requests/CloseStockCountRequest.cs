using System.ComponentModel.DataAnnotations;

namespace OrderPoint.Admin.StockCounts.Api.Requests;

internal sealed class CloseStockCountRequest
{
    [StringLength(200, ErrorMessage = "Note must be at most 200 characters.")]
    public string? Note { get; set; }
}