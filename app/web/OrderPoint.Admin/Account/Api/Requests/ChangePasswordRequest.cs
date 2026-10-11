using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OrderPoint.Admin.Account.Api.Requests;

internal sealed class ChangePasswordRequest
{
    [Required(ErrorMessage = "Current password is required.")]
    [StringLength(100, ErrorMessage = "Current password must be at most 100 characters.")]
    public string CurrentPassword { get; set; } = null!;

    [Required(ErrorMessage = "New password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "New password must be 8 to 100 characters.")]
    public string NewPassword { get; set; } = null!;

    [JsonIgnore]
    [Required(ErrorMessage = "Confirm the new password.")]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = null!;
}