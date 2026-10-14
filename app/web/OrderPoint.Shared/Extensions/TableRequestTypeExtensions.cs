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
            TableRequestType.Bill => "Bill please",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    internal static string GetIcon(this TableRequestType type)
    {
        return type switch
        {
            TableRequestType.CallBartender => Icons.Material.Filled.RoomService,
            TableRequestType.Bill => Icons.Material.Filled.ReceiptLong,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}