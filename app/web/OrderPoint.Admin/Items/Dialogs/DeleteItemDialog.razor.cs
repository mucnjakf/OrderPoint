using Microsoft.AspNetCore.Components;

namespace OrderPoint.Admin.Items.Dialogs;

public sealed partial class DeleteItemDialog
{
    [Parameter]
    public string ItemName { get; set; } = null!;
}