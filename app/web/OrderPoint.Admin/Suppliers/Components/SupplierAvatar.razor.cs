using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace OrderPoint.Admin.Suppliers.Components;

// The supplier's initials in a colour of their own, like a bartender without a photo: "Pivovar Split" is PS
public sealed partial class SupplierAvatar
{
    [Parameter]
    [EditorRequired]
    public string Name { get; set; } = null!;

    [Parameter]
    public Size Size { get; set; } = Size.Medium;

    private string[] Words => Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

    private string? FirstWord => Words.ElementAtOrDefault(0);

    private string? SecondWord => Words.ElementAtOrDefault(1);
}