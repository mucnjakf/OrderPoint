namespace OrderPoint.Admin.Tables.Api.Requests;

internal sealed class MoveFixtureRequest
{
    public Guid AreaId { get; set; }

    public int X { get; set; }

    public int Y { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }
}