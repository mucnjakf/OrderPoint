using System.Globalization;
using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Items.Dtos;

namespace OrderPoint.Admin.Items.Components;

// The stock one serving uses, with what it costs and earns
public sealed partial class ItemRecipePaper
{
    private const string CostStyle = "width: 80px; text-align: right; color: var(--mud-palette-text-secondary)";
    // Null while loading
    [Parameter]
    [EditorRequired]
    public ItemRecipeDto? Recipe { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback OnEditClick { get; set; }

    // "Cost €1.35 · Price €9.00 · Margin €7.65 (85%)"
    private string MarginText
    {
        get
        {
            string cost = (Recipe!.Cost ?? 0).ToString("C", CultureInfo.CurrentCulture);
            string price = Recipe.Price.ToString("C", CultureInfo.CurrentCulture);
            decimal margin = Recipe.Margin ?? 0;
            string marginText = margin.ToString("C", CultureInfo.CurrentCulture);
            string percent = (margin / Recipe.Price).ToString("P0", CultureInfo.CurrentCulture);

            return $"Cost {cost} · Price {price} · Margin {marginText} ({percent})";
        }
    }
}