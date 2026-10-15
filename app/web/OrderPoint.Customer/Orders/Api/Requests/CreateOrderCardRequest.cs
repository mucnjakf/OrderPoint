using System.ComponentModel.DataAnnotations;

namespace OrderPoint.Customer.Orders.Api.Requests;

internal sealed class CreateOrderCardRequest
{
    [Required(ErrorMessage = "Card number is required.")]
    [RegularExpression("^(?:[0-9] ?){12,18}[0-9]$", ErrorMessage = "Card number must have 13 to 19 digits.")]
    public string Number { get; set; } = string.Empty;

    [Required(ErrorMessage = "Expiry is required.")]
    [RegularExpression("^(0[1-9]|1[0-2])/[0-9]{2}$", ErrorMessage = "Expiry must be MM/YY.")]
    public string Expiry { get; set; } = string.Empty;

    [Required(ErrorMessage = "CVC is required.")]
    [RegularExpression("^[0-9]{3,4}$", ErrorMessage = "CVC must be 3 or 4 digits.")]
    public string Cvc { get; set; } = string.Empty;
}