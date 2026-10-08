using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Bartenders.Dtos;

namespace OrderPoint.Admin.Bartenders.Components;

public sealed partial class BartenderTeamStrip
{
    [Parameter]
    [EditorRequired]
    public IReadOnlyList<BartenderDto> ActiveBartenders { get; set; } = [];

    [Parameter]
    [EditorRequired]
    public int ActiveCount { get; set; }

    [Parameter]
    [EditorRequired]
    public int InactiveCount { get; set; }

    [Parameter]
    [EditorRequired]
    public BartenderDto? NewestBartender { get; set; }

    [Parameter]
    [EditorRequired]
    public bool IsLoadingActiveTeam { get; set; }

    [Parameter]
    [EditorRequired]
    public bool IsLoadingInactiveCount { get; set; }

    [Parameter]
    [EditorRequired]
    public bool IsLoadingNewestBartender { get; set; }

    private bool IsLoadingBalance => IsLoadingActiveTeam || IsLoadingInactiveCount;

    private int TotalCount => ActiveCount + InactiveCount;

    private int HiddenActiveCount => ActiveCount - ActiveBartenders.Count;

    private double ActivePercentage => TotalCount == 0 ? 0 : ActiveCount * 100.0 / TotalCount;
}