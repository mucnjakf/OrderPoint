using Microsoft.AspNetCore.Components;
using OrderPoint.Shared.Extensions;
using OrderPoint.Shared.Tables.Dtos;

namespace OrderPoint.Bartender.Tables.Components;

// A guest's request: what and since when, with Done; in the Orders strip it also names the table and opens it
public sealed partial class TableRequestItem
{
    [Parameter]
    [EditorRequired]
    public TableRequestDto Request { get; set; } = null!;

    [Parameter]
    public bool ShowsTable { get; set; }

    [Parameter]
    [EditorRequired]
    public bool IsCompleting { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<TableRequestDto> OnDone { get; set; }

    // Set in the Orders strip: the request opens its table on the map
    [Parameter]
    public EventCallback<TableRequestDto> OnClick { get; set; }

    private string Title => ShowsTable ? $"{Request.TableCode} · {Request.Type.GetLabel()}" : Request.Type.GetLabel();

    private string PaperClass => OnClick.HasDelegate ? "pa-2 pl-3 cursor-pointer" : "pa-2 pl-3";

    private async Task OnPaperClickAsync()
    {
        await OnClick.InvokeAsync(Request);
    }

    private async Task OnDoneClickAsync()
    {
        await OnDone.InvokeAsync(Request);
    }
}