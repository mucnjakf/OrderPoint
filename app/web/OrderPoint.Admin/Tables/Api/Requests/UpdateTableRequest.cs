using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using OrderPoint.Shared.Tables.Enumerations;

namespace OrderPoint.Admin.Tables.Api.Requests;

// The size is edited in the same dialog but saved with the position endpoint
internal sealed class UpdateTableRequest
{
    [Required(ErrorMessage = "Code is required.")]
    [StringLength(10, ErrorMessage = "Code must be at most 10 characters.")]
    [RegularExpression("^[A-Z0-9-]+$", ErrorMessage = "Use capital letters, digits and hyphens.")]
    public string Code { get; set; } = null!;

    [Range(1, 20, ErrorMessage = "Seats must be between 1 and 20.")]
    public int Seats { get; set; } = 4;

    public TableStatus Status { get; set; }

    public TableShape Shape { get; set; }

    [JsonIgnore]
    [Range(1, 12, ErrorMessage = "Width must be between 1 and 12.")]
    public int Width { get; set; }

    [JsonIgnore]
    [Range(1, 12, ErrorMessage = "Height must be between 1 and 12.")]
    public int Height { get; set; }
}