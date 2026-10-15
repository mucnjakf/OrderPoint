using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace OrderPoint.Admin.Shared.Components;

// Keeps a list as tall as before while a page loads, and while on later pages, so the pager below never moves
public sealed partial class StableHeight : IAsyncDisposable
{
    private const string ModulePath = "./Shared/Components/StableHeight.razor.js";

    private const string FramedStyle =
        "background-color: rgba(var(--mud-palette-text-primary-rgb), 0.02); " +
        "border: 1px solid var(--mud-palette-lines-default); " +
        "border-radius: var(--mud-default-borderradius); overflow: hidden; " +
        "display: flex; flex-direction: column;";

    private IJSObjectReference? _module;

    private bool _wasLoading;

    private bool _isReleasePending;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public bool IsLoading { get; set; }

    [Parameter]
    [EditorRequired]
    public bool KeepHeight { get; set; }

    // A subtle background and border, so the held space reads as part of the list; without padding the rows'
    // own paper drops its border and the frame becomes the list's edge
    [Parameter]
    public bool IsFramed { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    [EditorRequired]
    public RenderFragment ChildContent { get; set; } = null!;

    private ElementReference Element { get; set; }

    // Runs before the skeleton replaces the rows, so the rows' height is the one held
    protected override async Task OnParametersSetAsync()
    {
        bool hasLoadingStarted = IsLoading && !_wasLoading;
        bool hasLoadingEnded = !IsLoading && _wasLoading;

        _wasLoading = IsLoading;

        if (hasLoadingEnded && !KeepHeight)
        {
            _isReleasePending = true;
        }

        if (hasLoadingStarted && _module is not null)
        {
            await _module.InvokeVoidAsync("holdHeight", Element);
        }
    }

    // Released only once the new rows are rendered, otherwise the skeleton would shrink first
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _module = await JsRuntime.InvokeAsync<IJSObjectReference>("import", ModulePath);
        }

        if (!_isReleasePending || _module is null)
        {
            return;
        }

        _isReleasePending = false;

        await _module.InvokeVoidAsync("releaseHeight", Element);
    }

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
}