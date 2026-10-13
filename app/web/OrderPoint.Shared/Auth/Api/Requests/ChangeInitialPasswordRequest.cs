using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using OrderPoint.Shared.Auth.Enumerations;

namespace OrderPoint.Shared.Auth.Api.Requests;

// Email, temporary password and role come from the sign-in attempt that asked for a new password
internal sealed class ChangeInitialPasswordRequest
{
    public string Email { get; set; } = null!;

    public string CurrentPassword { get; set; } = null!;

    [Required(ErrorMessage = "New password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "New password must be 8 to 100 characters.")]
    public string NewPassword { get; set; } = null!;

    [JsonIgnore]
    [Required(ErrorMessage = "Confirm the new password.")]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = null!;

    public UserRole Role { get; set; }
}