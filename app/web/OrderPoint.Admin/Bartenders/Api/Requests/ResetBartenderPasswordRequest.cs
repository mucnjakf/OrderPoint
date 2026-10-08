using System.ComponentModel.DataAnnotations;

namespace OrderPoint.Admin.Bartenders.Api.Requests;

internal sealed class ResetBartenderPasswordRequest
{
    [Required(ErrorMessage = "Temporary password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Temporary password must be 8 to 100 characters.")]
    public string Password { get; set; } = null!;
}