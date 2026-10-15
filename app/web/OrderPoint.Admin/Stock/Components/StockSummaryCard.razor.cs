using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace OrderPoint.Admin.Stock.Components;

// A number above the stock sheet; with OnClick it filters the sheet, like a toggle
public sealed partial class StockSummaryCard
{
    private const int CardHeight = 150;

    [Parameter]
    [EditorRequired]
    public string Label { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public string Icon { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public Color Color { get; set; }

    [Parameter]
    [EditorRequired]
    public decimal Value { get; set; }

    [Parameter]
    [EditorRequired]
    public string Text { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public string Caption { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public bool IsLoading { get; set; }

    [Parameter]
    public bool IsSelected { get; set; }

    [Parameter]
    public EventCallback OnClick { get; set; }
}