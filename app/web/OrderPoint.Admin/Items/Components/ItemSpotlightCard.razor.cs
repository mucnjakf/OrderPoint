using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Items.Dtos;

namespace OrderPoint.Admin.Items.Components;

public sealed partial class ItemSpotlightCard
{
    [Parameter]
    [EditorRequired]
    public string Label { get; set; }

    [Parameter]
    [EditorRequired]
    public string Icon { get; set; }

    [Parameter]
    [EditorRequired]
    public Color Color { get; set; }

    [Parameter]
    [EditorRequired]
    public ItemDto? Item { get; set; }

    [Parameter]
    [EditorRequired]
    public bool IsLoading { get; set; }

    [Parameter]
    public string? Caption { get; set; }

    [Parameter]
    [EditorRequired]
    public string EmptyText { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<ItemDto> OnClick { get; set; }

    private async Task OnCardClickAsync()
    {
        if (Item is null)
        {
            return;
        }

        await OnClick.InvokeAsync(Item);
    }
}