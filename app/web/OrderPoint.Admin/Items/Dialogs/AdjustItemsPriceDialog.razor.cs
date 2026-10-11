using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Items.Api.Requests;
using OrderPoint.Admin.Items.Dtos;
using OrderPoint.Admin.Items.Enumerations;
using OrderPoint.Admin.Shared.Extensions;

namespace OrderPoint.Admin.Items.Dialogs;

public sealed partial class AdjustItemsPriceDialog
{
    private const decimal MinPercent = -100;

    [Parameter]
    public IReadOnlyList<ItemDto> Items { get; set; } = [];

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private PriceAdjustmentMode Mode { get; set; } = PriceAdjustmentMode.ChangeByPercent;

    private decimal? Value { get; set; }

    private string ValueAdornmentText => Mode is PriceAdjustmentMode.ChangeByPercent ? "%" : "€";

    private string ValueHelperText => Mode is PriceAdjustmentMode.Set
        ? "The new price of every selected item."
        : "Use a negative value to lower the prices.";

    private string? ValueErrorText => Value switch
    {
        null => null,
        <= 0 when Mode is PriceAdjustmentMode.Set => "Price must be positive.",
        0 => "Value must not be zero.",
        <= MinPercent when Mode is PriceAdjustmentMode.ChangeByPercent => "Value must be greater than -100.",
        _ when Items.Any(item => GetNewPrice(item) <= 0) => "Some prices would not be positive.",
        _ => null
    };

    private bool HasValueError => ValueErrorText is not null;

    private bool IsValid => Value is not null && !HasValueError;

    private decimal GetNewPrice(ItemDto item)
    {
        return Value is null ? item.Price : Mode.Apply(item.Price, Value.Value);
    }

    private Color GetNewPriceColor(ItemDto item)
    {
        return GetNewPrice(item) > 0 ? Color.Default : Color.Error;
    }

    private void Confirm()
    {
        if (!IsValid)
        {
            return;
        }

        var request = new AdjustItemsPriceRequest
        {
            Ids = [.. Items.Select(item => item.Id)],
            Mode = Mode,
            Value = Value!.Value
        };

        MudDialogInstance.Close(DialogResult.Ok(request));
    }

    private void Cancel()
    {
        MudDialogInstance.Cancel();
    }
}