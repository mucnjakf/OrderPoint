namespace OrderPoint.Admin.StockCounts.Dtos;

// A line on the count sheet with what is typed into it; a class, so the row keeps its fields while live reloads
// replace the line
public sealed class StockCountEntryDto(StockCountLineDto line)
{
    public StockCountLineDto Line { get; set; } = line;

    public decimal? FullPacks { get; set; } = GetFullPacks(line);

    // What is left in opened packs, in the base unit
    public decimal? OpenedAmount { get; set; } = GetOpenedAmount(line);

    public bool IsSaving { get; set; }

    // Null when both fields are empty, which clears the line
    internal decimal? Quantity => FullPacks is null && OpenedAmount is null
        ? null
        : (FullPacks ?? 0) * Line.PackSize + (OpenedAmount ?? 0);

    // Back to what the line holds, when saving was refused
    internal void Reset()
    {
        FullPacks = GetFullPacks(Line);
        OpenedAmount = GetOpenedAmount(Line);
    }

    private static decimal? GetFullPacks(StockCountLineDto line)
    {
        return line.CountedQuantity is { } counted ? Math.Floor(counted / line.PackSize) : null;
    }

    private static decimal? GetOpenedAmount(StockCountLineDto line)
    {
        if (line.CountedQuantity is not { } counted || line.PackSize == 1)
        {
            return null;
        }

        decimal opened = counted - Math.Floor(counted / line.PackSize) * line.PackSize;

        return opened == 0 ? null : opened;
    }
}