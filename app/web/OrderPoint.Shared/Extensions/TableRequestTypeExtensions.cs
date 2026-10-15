using MudBlazor;
using OrderPoint.Shared.Tables.Enumerations;

namespace OrderPoint.Shared.Extensions;

internal static class TableRequestTypeExtensions
{
    internal static string GetLabel(this TableRequestType type)
    {
        return type switch
        {
            TableRequestType.CallBartender => "Call bartender",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    internal static string GetIcon(this TableRequestType type)
    {
        return type switch
        {
            TableRequestType.CallBartender => Icons.Material.Filled.RoomService,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}