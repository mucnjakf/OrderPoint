using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using OrderPoint.Shared.Tables.Enumerations;

namespace OrderPoint.Admin.Tables.Api.Requests;

// The size is edited in the same dialog but saved with the position endpoint
internal sealed class UpdateFixtureRequest
{
    public FixtureType Type { get; set; }

    [StringLength(30, ErrorMessage = "Label must be at most 30 characters.")]
    public string? Label { get; set; }

    [JsonIgnore]
    [Range(1, 48, ErrorMessage = "Width must be between 1 and 48.")]
    public int Width { get; set; }

    [JsonIgnore]
    [Range(1, 48, ErrorMessage = "Height must be between 1 and 48.")]
    public int Height { get; set; }
}