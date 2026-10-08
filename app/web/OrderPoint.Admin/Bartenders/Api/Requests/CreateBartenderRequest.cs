using System.ComponentModel.DataAnnotations;
using OrderPoint.Admin.Bartenders.Enumerations;

namespace OrderPoint.Admin.Bartenders.Api.Requests;

internal sealed class CreateBartenderRequest
{
    [Required(ErrorMessage = "First name is required.")]
    [StringLength(30, ErrorMessage = "First name must be at most 30 characters.")]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(30, ErrorMessage = "Last name must be at most 30 characters.")]
    public string LastName { get; set; } = null!;

    [Required(ErrorMessage = "Email is required.")]
    [StringLength(100, ErrorMessage = "Email must be at most 100 characters.")]
    [EmailAddress(ErrorMessage = "Email is invalid.")]
    public string Email { get; set; } = null!;

    [StringLength(20, ErrorMessage = "Phone number must be at most 20 characters.")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    public BartenderStatus Status { get; set; }

    [StringLength(500, ErrorMessage = "Notes must be at most 500 characters.")]
    public string? Notes { get; set; }

    [StringLength(200, ErrorMessage = "Image URL must be at most 200 characters.")]
    public string? ImageUrl { get; set; }
}