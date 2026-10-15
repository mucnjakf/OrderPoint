namespace OrderPoint.Bartender.Tables.Components;

// Shared by the tables page and its skeleton, so loading never moves the tiles
internal static class TableTilesLayout
{
    internal const string GridStyle =
        "display: grid; grid-template-columns: repeat(auto-fill, minmax(76px, 1fr)); gap: 8px;";
}