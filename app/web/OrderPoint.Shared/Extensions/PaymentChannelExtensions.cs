using MudBlazor;
using OrderPoint.Shared.Orders.Enumerations;

namespace OrderPoint.Shared.Extensions;

internal static class PaymentChannelExtensions
{
    internal static string GetLabel(this PaymentChannel channel)
    {
        return channel switch
        {
            PaymentChannel.Cash => "Cash",
            PaymentChannel.CardAtRegister => "Card at the register",
            _ => "Card online"
        };
    }

    internal static string GetIcon(this PaymentChannel channel)
    {
        return channel switch
        {
            PaymentChannel.Cash => Icons.Material.Filled.Payments,
            PaymentChannel.CardAtRegister => Icons.Material.Filled.CreditCard,
            _ => Icons.Material.Filled.PhoneIphone
        };
    }
}