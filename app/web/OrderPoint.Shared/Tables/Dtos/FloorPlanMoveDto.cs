namespace OrderPoint.Shared.Tables.Dtos;

// A piece dropped on the floor plan editor: its new top-left cell
public sealed record FloorPlanMoveDto(Guid Id, int X, int Y);