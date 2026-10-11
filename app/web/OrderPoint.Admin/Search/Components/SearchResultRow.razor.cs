using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using OrderPoint.Admin.Search.Dtos;

namespace OrderPoint.Admin.Search.Components;

public sealed partial class SearchResultRow
{
    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public SearchResultDto Result { get; set; } = null!;

    [Parameter]
    public bool IsActive { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<SearchResultDto> OnClick { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<SearchResultDto> OnMouseEnter { get; set; }

    private ElementReference RowElement { get; set; }

    private bool WasActive { get; set; }

    private string RowStyle => IsActive
        ? "border-radius: var(--mud-default-borderradius); background-color: var(--mud-palette-table-hover);"
        : "border-radius: var(--mud-default-borderradius);";

    private string FirstName => Result.Title.Split(' ', 2)[0];

    private string? LastName => Result.Title.Split(' ', 2).ElementAtOrDefault(1);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        bool isNewlyActive = IsActive && !WasActive;

        WasActive = IsActive;

        if (isNewlyActive)
        {
            await JsRuntime.InvokeVoidAsync("scrollSearchResultIntoView", RowElement);
        }
    }

    private async Task OnClickAsync()
    {
        await OnClick.InvokeAsync(Result);
    }

    private async Task OnMouseEnterAsync()
    {
        await OnMouseEnter.InvokeAsync(Result);
    }
}