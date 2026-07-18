using Microsoft.AspNetCore.Components;

namespace OrderPoint.Admin.Items.Pages.Update.Components;

public sealed partial class UpdateItemDeletePaper
{
    [Parameter]
    [EditorRequired]
    public EventCallback OnDeleteClick { get; set; }
}