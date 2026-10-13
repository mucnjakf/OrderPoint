using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace OrderPoint.Customer.Shared.Extensions;

internal static class SnackbarExtensions
{
    private const string CartUrl = "/cart";

    // "2× Mojito added" with a View button that opens the cart
    internal static void AddCartConfirmation(
        this ISnackbar snackbar,
        string message,
        NavigationManager navigationManager)
    {
        snackbar.Add(message, Severity.Success, options =>
        {
            options.Action = "View";
            options.ActionVariant = Variant.Text;
            options.OnClick = _ =>
            {
                navigationManager.NavigateTo(CartUrl);

                return Task.CompletedTask;
            };
        });
    }
}