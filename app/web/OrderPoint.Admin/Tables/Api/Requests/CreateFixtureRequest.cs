using System.ComponentModel.DataAnnotations;
using OrderPoint.Shared.Tables.Enumerations;

namespace OrderPoint.Admin.Tables.Api.Requests;

internal sealed class CreateFixtureRequest
{
    public Guid AreaId { get; set; }

    public FixtureType Type { get; set; }

    [StringLength(30, ErrorMessage = "Label must be at most 30 characters.")]
    public string? Label { get; set; }

    public int X { get; set; }

    public int Y { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }
}