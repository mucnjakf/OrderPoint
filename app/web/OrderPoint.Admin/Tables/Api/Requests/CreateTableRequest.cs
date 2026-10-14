using System.ComponentModel.DataAnnotations;
using OrderPoint.Shared.Tables.Enumerations;

namespace OrderPoint.Admin.Tables.Api.Requests;

internal sealed class CreateTableRequest
{
    [Required(ErrorMessage = "Code is required.")]
    [StringLength(10, ErrorMessage = "Code must be at most 10 characters.")]
    [RegularExpression("^[A-Z0-9-]+$", ErrorMessage = "Use capital letters, digits and hyphens.")]
    public string Code { get; set; } = null!;

    [Range(1, 20, ErrorMessage = "Seats must be between 1 and 20.")]
    public int Seats { get; set; } = 4;

    public TableStatus Status { get; set; }

    public TableShape Shape { get; set; }

    // The place is set by the page from the free spot it found, not entered in the dialog
    public Guid AreaId { get; set; }

    public int X { get; set; }

    public int Y { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }
}