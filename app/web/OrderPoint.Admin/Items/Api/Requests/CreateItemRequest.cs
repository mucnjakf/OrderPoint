using System.ComponentModel.DataAnnotations;

namespace OrderPoint.Admin.Items.Api.Requests;

internal sealed class CreateItemRequest
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(30, ErrorMessage = "Name must be at most 30 characters.")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(200, ErrorMessage = "Description must be at most 200 characters.")]
    public string Description { get; set; } = null!;

    [Required(ErrorMessage = "Portion is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Portion must be positive.")]
    public double Portion { get; set; }

    [Required(ErrorMessage = "Price is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be positive.")]
    public decimal Price { get; set; }

    [StringLength(200, ErrorMessage = "Image URL must be at most 200 characters.")]
    public string? ImageUrl { get; set; }

    [Range(
        typeof(Guid),
        "00000001-0000-0000-0000-000000000000",
        "ffffffff-ffff-ffff-ffff-ffffffffffff",
        ErrorMessage = "Category is required.")]
    public Guid CategoryId { get; set; }
}