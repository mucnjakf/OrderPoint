using System.ComponentModel.DataAnnotations;

namespace OrderPoint.Bar.Orders.Api.Requests;

internal sealed class DeclineOrderRequest
{
    [Required(ErrorMessage = "Reason is required.")]
    [StringLength(200, ErrorMessage = "Reason must be at most 200 characters.")]
    public string Reason { get; set; } = string.Empty;
}