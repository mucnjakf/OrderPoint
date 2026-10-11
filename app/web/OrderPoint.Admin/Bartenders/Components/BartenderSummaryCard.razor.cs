using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Bartenders.Dtos;

namespace OrderPoint.Admin.Bartenders.Components;

public sealed partial class BartenderSummaryCard
{
    [Parameter]
    [EditorRequired]
    public BartenderDto Bartender { get; set; } = null!;

    private string BartenderName => $"{Bartender.FirstName} {Bartender.LastName}";
}