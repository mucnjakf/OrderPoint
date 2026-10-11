using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Shared.Components;

namespace OrderPoint.Admin.Categories.Components;

public sealed partial class TopCategoriesSkeleton
{
    [Parameter]
    [EditorRequired]
    public int Count { get; set; }

    private static string CardStyle => $"height: {HighlightCard.DefaultHeight}px;";
}