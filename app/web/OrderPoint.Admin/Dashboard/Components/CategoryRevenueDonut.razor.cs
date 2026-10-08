using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Dashboard.Dtos;

namespace OrderPoint.Admin.Dashboard.Components;

public sealed partial class CategoryRevenueDonut
{
    private const string ChartHeight = "220px";

    private static readonly DonutChartOptions Options = new()
    {
        ShowLegend = true
    };

    [Parameter]
    [EditorRequired]
    public IReadOnlyList<CategoryRevenueDto> CategoryRevenue { get; set; } = [];

    private List<ChartSeries<double>> Series { get; set; } = [];

    private string[] Labels { get; set; } = [];

    protected override void OnParametersSet()
    {
        Series =
        [
            new ChartSeries<double>
            {
                Name = "Revenue",
                Data = CategoryRevenue.Select(category => (double)category.Revenue).ToArray()
            }
        ];

        Labels = CategoryRevenue.Select(category => category.CategoryName).ToArray();
    }
}