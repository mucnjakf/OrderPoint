using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using OrderPoint.Admin.Categories.Enumerations;
using OrderPoint.Admin.Shared.Dtos;

namespace OrderPoint.Admin.Categories.Api.Requests;

internal sealed class UpdateCategoryRequest
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(30, ErrorMessage = "Name must be at most 30 characters.")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(100, ErrorMessage = "Description must be at most 100 characters.")]
    public string Description { get; set; } = null!;

    [Required(ErrorMessage = "Status is required.")]
    public CategoryStatus Status { get; set; }

    [JsonIgnore]
    public ImageFileDto? Image { get; set; }

    [JsonIgnore]
    public bool RemoveImage { get; set; }
}