using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Dashboard.Dtos;

namespace OrderPoint.Admin.Dashboard.Components;

public sealed partial class BartenderLeaderboard
{
    [Parameter]
    [EditorRequired]
    public IReadOnlyList<BartenderLeaderboardEntryDto> Entries { get; set; } = [];

    private static string GetDeclineRateText(BartenderLeaderboardEntryDto entry)
    {
        double declineRate = entry.HandledOrdersCount == 0
            ? 0
            : entry.DeclinedOrdersCount * 100.0 / entry.HandledOrdersCount;

        return $"{declineRate:0}%";
    }
}