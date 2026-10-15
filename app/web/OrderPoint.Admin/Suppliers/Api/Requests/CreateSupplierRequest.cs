using System.ComponentModel.DataAnnotations;
using OrderPoint.Admin.Suppliers.Enumerations;

namespace OrderPoint.Admin.Suppliers.Api.Requests;

internal sealed class CreateSupplierRequest
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, ErrorMessage = "Name must be at most 100 characters.")]
    public string Name { get; set; } = null!;

    [StringLength(100, ErrorMessage = "Contact must be at most 100 characters.")]
    public string? ContactName { get; set; }

    [StringLength(30, ErrorMessage = "Phone must be at most 30 characters.")]
    public string? Phone { get; set; }

    // A cleared field sends no email rather than an empty, invalid one
    [EmailAddress(ErrorMessage = "Email is invalid.")]
    [StringLength(100, ErrorMessage = "Email must be at most 100 characters.")]
    public string? Email
    {
        get;
        set => field = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    [StringLength(200, ErrorMessage = "Note must be at most 200 characters.")]
    public string? Note { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    public SupplierStatus Status { get; set; }
}