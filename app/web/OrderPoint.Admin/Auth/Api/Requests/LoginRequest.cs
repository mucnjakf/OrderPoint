using System.ComponentModel.DataAnnotations;
using OrderPoint.Admin.Auth.Enumerations;

namespace OrderPoint.Admin.Auth.Api.Requests;

internal sealed class LoginRequest
{
    [Required(ErrorMessage = "Email is required.")]
    [StringLength(100, ErrorMessage = "Email must be at most 100 characters.")]
    [EmailAddress(ErrorMessage = "Email is invalid.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, ErrorMessage = "Password must be at most 100 characters.")]
    public string Password { get; set; } = null!;

    // The admin app signs in admins only; the API treats any other account as invalid credentials
    public UserRole Role => UserRole.Admin;
}