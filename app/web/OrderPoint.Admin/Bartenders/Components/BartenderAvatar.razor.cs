using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace OrderPoint.Admin.Bartenders.Components;

public sealed partial class BartenderAvatar
{
    private static readonly Color[] AvatarColors =
    [
        Color.Primary,
        Color.Secondary,
        Color.Tertiary,
        Color.Info,
        Color.Success,
        Color.Warning
    ];

    [Parameter]
    [EditorRequired]
    public string? FirstName { get; set; }

    [Parameter]
    [EditorRequired]
    public string? LastName { get; set; }

    [Parameter]
    [EditorRequired]
    public string? ImageUrl { get; set; }

    [Parameter]
    public Size Size { get; set; } = Size.Medium;

    [Parameter]
    public string? Class { get; set; }

    private string Initials => $"{GetInitial(FirstName)}{GetInitial(LastName)}";

    private Color AvatarColor => AvatarColors[GetStableHash($"{FirstName} {LastName}") % AvatarColors.Length];

    private static string GetInitial(string? name)
    {
        return string.IsNullOrWhiteSpace(name)
            ? string.Empty
            : char.ToUpperInvariant(name.Trim()[0]).ToString();
    }

    private static int GetStableHash(string value)
    {
        uint hash = 0;

        foreach (char character in value)
        {
            hash = unchecked(hash * 31 + character);
        }

        return (int)(hash % int.MaxValue);
    }
}