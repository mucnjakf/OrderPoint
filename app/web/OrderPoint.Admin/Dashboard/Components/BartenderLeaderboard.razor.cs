using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Dashboard.Dtos;

namespace OrderPoint.Admin.Dashboard.Components;

public sealed partial class BartenderLeaderboard
{
    [Parameter]
    [EditorRequired]
    public IReadOnlyList<BartenderLeaderboardEntryDto> Entries { get; set; } = [];

    [Parameter]
    [EditorRequired]
    public EventCallback<BartenderLeaderboardEntryDto> OnBartenderClick { get; set; }

    private Guid? HoveredBartenderId { get; set; }

    private string? GetRowStyle(BartenderLeaderboardEntryDto entry)
    {
        return entry.BartenderId == HoveredBartenderId ? "background-color: var(--mud-palette-table-hover);" : null;
    }

    private async Task OnBartenderClickAsync(BartenderLeaderboardEntryDto entry)
    {
        await OnBartenderClick.InvokeAsync(entry);
    }

    private static string GetDeclineRateText(BartenderLeaderboardEntryDto entry)
    {
        double declineRate = entry.HandledOrdersCount == 0
            ? 0
            : entry.DeclinedOrdersCount * 100.0 / entry.HandledOrdersCount;

        return $"{declineRate:0}%";
    }
}