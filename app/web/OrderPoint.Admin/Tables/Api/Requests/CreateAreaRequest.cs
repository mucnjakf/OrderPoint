using System.ComponentModel.DataAnnotations;

namespace OrderPoint.Admin.Tables.Api.Requests;

internal sealed class CreateAreaRequest
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(30, ErrorMessage = "Name must be at most 30 characters.")]
    public string Name { get; set; } = null!;

    [Range(4, 48, ErrorMessage = "Columns must be between 4 and 48.")]
    public int Columns { get; set; } = 24;

    [Range(4, 48, ErrorMessage = "Rows must be between 4 and 48.")]
    public int Rows { get; set; } = 16;
}