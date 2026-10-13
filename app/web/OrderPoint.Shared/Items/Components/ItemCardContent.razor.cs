using System.Globalization;
using Microsoft.AspNetCore.Components;
using OrderPoint.Shared.Extensions;

namespace OrderPoint.Shared.Items.Components;

// The item card's picture and text, read like a menu; the Admin's item grid and the bartender app's item page
// put it in their own MudCard
public sealed partial class ItemCardContent
{
    private const string SecondaryTextStyle = "color: var(--mud-palette-text-secondary);";

    private const string Separator = " · ";

    private const string ClampedDescriptionStyle =
        $"{SecondaryTextStyle} display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; " +
        "overflow: hidden;";

    [Parameter]
    [EditorRequired]
    public string? ImageUrl { get; set; }

    [Parameter]
    [EditorRequired]
    public bool IsActive { get; set; }

    [Parameter]
    [EditorRequired]
    public string CategoryName { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public string? CategoryImageUrl { get; set; }

    [Parameter]
    [EditorRequired]
    public string Name { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public string Description { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public decimal Price { get; set; }

    // Null leaves it out (guests, who see no unit next to it)
    [Parameter]
    public double? Portion { get; set; }

    // Staff see how often the item was ordered; null leaves it out (guests)
    [Parameter]
    public int? OrdersCount { get; set; }

    [Parameter]
    public bool ShowsStatus { get; set; } = true;

    // Cards in a grid cut the description after 2 lines; a details page shows all of it
    [Parameter]
    public bool IsDescriptionClamped { get; set; } = true;

    private string DescriptionStyle => IsDescriptionClamped ? ClampedDescriptionStyle : SecondaryTextStyle;

    private string PriceText => Price.ToString("C", CultureInfo.CurrentCulture);

    private string PortionText => (Portion ?? 0).ToString(CultureInfo.CurrentCulture);

    private string OrdersCountText => (OrdersCount ?? 0).ToCountText("order", "orders");
}