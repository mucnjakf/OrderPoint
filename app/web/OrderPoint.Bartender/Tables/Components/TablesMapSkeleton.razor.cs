using OrderPoint.Bartender.Tables.Pages;

namespace OrderPoint.Bartender.Tables.Components;

// As tall as the map will be: the whole screen between the app bar and the bottom navigation
public sealed partial class TablesMapSkeleton
{
    private const string SkeletonStyle = $"height: calc({TablesPage.ScreenHeight});";
}