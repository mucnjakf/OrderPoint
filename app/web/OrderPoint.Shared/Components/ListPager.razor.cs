using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using OrderPoint.Shared.Dtos;

namespace OrderPoint.Shared.Components;

// Rendered while loading too (disabled), so it stays where it is
public sealed partial class ListPager<TItem> : IAsyncDisposable
{
    private const string ModulePath = "./_content/OrderPoint.Shared/Components/ListPager.razor.js";

    private IJSObjectReference? _module;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public PaginationDto<TItem>? Pagination { get; set; }

    [Parameter]
    [EditorRequired]
    public bool IsLoading { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<int> OnPageChanged { get; set; }

    private ElementReference Element { get; set; }

    public async ValueTask DisposeAsync()
    {
        if (_module is null)
        {
            return;
        }

        try
        {
            await _module.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
            // The browser is already gone, and the module with it
        }
    }

    // Scrolls while the page loads; the list keeps its height, so the scroll lands where the new rows appear
    private async Task OnPageSelectedAsync(int pageNumber)
    {
        _module ??= await JsRuntime.InvokeAsync<IJSObjectReference>("import", ModulePath);

        await _module.InvokeVoidAsync("scrollToTop", Element);
        await OnPageChanged.InvokeAsync(pageNumber);
    }
}