namespace OrderPoint.Admin.Dashboard.Components;

public sealed partial class CategoryRevenueDonutSkeleton
{
    private const int CategoriesCount = 3;

    private static string DonutContainerStyle => "position: relative; width: 170px; height: 170px; flex-shrink: 0;";

    private static string HoleStyle =>
        "position: absolute; width: 112px; height: 112px; border-radius: 50%; " +
        "background-color: var(--mud-palette-surface);";
}