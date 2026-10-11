using Microsoft.AspNetCore.Components;

namespace OrderPoint.Admin.Search.Components;

public sealed partial class SearchTrigger
{
    [Parameter]
    [EditorRequired]
    public EventCallback OnClick { get; set; }

    private async Task OnClickAsync()
    {
        await OnClick.InvokeAsync();
    }
}