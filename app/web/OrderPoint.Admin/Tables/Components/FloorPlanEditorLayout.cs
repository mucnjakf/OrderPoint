namespace OrderPoint.Admin.Tables.Components;

// Shared by the editor page and its skeleton, so loading never moves the panel
internal static class FloorPlanEditorLayout
{
    // The toolbar above the map only; the panel starts level with the map's top border
    internal const string GridStyle =
        "display: grid; grid-template-columns: minmax(0, 1fr) 360px; gap: 12px; align-items: start;";

    internal const string ToolbarStyle = "grid-column: 1; grid-row: 1;";

    internal const string MapStyle = "grid-column: 1; grid-row: 2;";

    internal const string PanelStyle = "grid-column: 2; grid-row: 2; position: sticky; top: 80px;";
}