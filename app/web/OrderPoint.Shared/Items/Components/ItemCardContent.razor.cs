using System.Globalization;
using Microsoft.AspNetCore.Components;
using OrderPoint.Shared.Extensions;

namespace OrderPoint.Shared.Items.Components;

// The item card's picture and text, read like a menu; the Admin's item grid and the bartender app's item page
// put it in their own MudCard
public sealed partial class ItemCardContent
{
    private const string SecondaryTextStyle = "color: var(--mud-palette-text-secondary);";

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

    [Parameter]
    [EditorRequired]
    public double Portion { get; set; }

    [Parameter]
    [EditorRequired]
    public int OrdersCount { get; set; }

    // Cards in a grid cut the description after 2 lines; a details page shows all of it
    [Parameter]
    public bool IsDescriptionClamped { get; set; } = true;

    private string DescriptionStyle => IsDescriptionClamped ? ClampedDescriptionStyle : SecondaryTextStyle;

    private string PriceText => Price.ToString("C", CultureInfo.CurrentCulture);

    private string PortionText => Portion.ToString(CultureInfo.CurrentCulture);

    private string OrdersCountText => OrdersCount.ToCountText("order", "orders");
}