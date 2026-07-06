using System.ComponentModel.DataAnnotations;
using OrderPoint.Admin.Enumerations;

namespace OrderPoint.Admin.ApiClients.Requests;

internal sealed class CreateCategoryRequest
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(30, ErrorMessage = "Name must be at most 30 characters.")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(100, ErrorMessage = "Name must be at most 100 characters.")]
    public string Description { get; set; } = null!;

    [Required(ErrorMessage = "Status is required.")]
    public CategoryStatus Status { get; set; }

    [StringLength(200, ErrorMessage = "Image URL must be at most 200 characters.")]
    public string? ImageUrl { get; set; }
}