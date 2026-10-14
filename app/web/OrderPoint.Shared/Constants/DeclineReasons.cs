namespace OrderPoint.Shared.Constants;

// Quick picks in the decline dialogs, so a busy bartender rarely has to type
internal static class DeclineReasons
{
    internal const int MaxLength = 200;

    internal static readonly IReadOnlyList<string> QuickPicks =
    [
        "Out of stock",
        "Bar is closing",
        "Please ask at the bar"
    ];
}