using System.Globalization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Items.Api.Requests;
using OrderPoint.Admin.Items.Dtos;
using OrderPoint.Admin.Stock.Api;
using OrderPoint.Admin.Stock.Dtos;
using OrderPoint.Admin.Stock.Enumerations;
using OrderPoint.Admin.Stock.Sorting;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Services;

namespace OrderPoint.Admin.Items.Dialogs;

// The whole recipe at once; returns the new lines, the page saves them
public sealed partial class EditRecipeDialog
{
    // Like the API
    private const int MaxLines = 30;

    private const int SearchResultsCount = 10;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private StockApiClient StockApiClient { get; set; } = null!;

    [Parameter]
    public ItemDto Item { get; set; } = null!;

    [Parameter]
    public ItemRecipeDto Recipe { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private List<RecipeRow> Rows { get; set; } = [];

    private decimal Cost => Rows.Sum(GetCost);

    private decimal Margin => Item.Price - Cost;

    private string CostText =>
        $"Cost {Cost.ToString("C", CultureInfo.CurrentCulture)} · " +
        $"Price {Item.Price.ToString("C", CultureInfo.CurrentCulture)}";

    private string MarginText =>
        $"{Margin.ToString("C", CultureInfo.CurrentCulture)} " +
        $"({(Margin / Item.Price).ToString("P0", CultureInfo.CurrentCulture)})";

    private string? MarginStyle => Margin < 0 ? "color: var(--mud-palette-error); font-weight: 600" : "font-weight: 600";

    // Every row picked, with an amount, each stock item once
    private bool CanSave => Rows.All(row => row.Ingredient is not null && row.Quantity > 0 && !IsDuplicate(row));

    protected override void OnInitialized()
    {
        Rows = Recipe.Lines
            .Select(line => new RecipeRow
            {
                Ingredient = new RecipeIngredientDto(
                    line.StockItemId,
                    line.StockItemName,
                    line.Group,
                    line.Unit,
                    line.PackSize,
                    line.PackCost),
                Quantity = line.Quantity
            })
            .ToList();

        if (Rows.Count == 0)
        {
            AddRow();
        }
    }

    private void AddRow()
    {
        Rows.Add(new RecipeRow());
    }

    private bool IsDuplicate(RecipeRow row)
    {
        return row.Ingredient is not null
               && Rows.Any(other => other != row && other.Ingredient?.StockItemId == row.Ingredient.StockItemId);
    }

    private static decimal GetCost(RecipeRow row)
    {
        return row.Ingredient is null ? 0 : row.Quantity * row.Ingredient.UnitCost;
    }

    private async Task<IEnumerable<RecipeIngredientDto>> SearchIngredientsAsync(
        string? value,
        CancellationToken cancellationToken)
    {
        PaginationDto<StockItemDto>? stockItems = await ApiService.ExecuteAsync(
            () => StockApiClient.GetStockItemsAsync(
                pageNumber: 1,
                SearchResultsCount,
                nameof(StockItemSortBy.NameAsc),
                value,
                status: StockItemStatus.Active,
                cancellationToken: cancellationToken),
            cancellationToken);

        return (stockItems?.Items ?? [])
            .Select(stockItem => new RecipeIngredientDto(
                stockItem.Id,
                stockItem.Name,
                stockItem.Group,
                stockItem.Unit,
                stockItem.PackSize,
                stockItem.PackCost));
    }

    private void Save()
    {
        var request = new SetItemRecipeRequest(Rows
            .Select(row => new SetItemRecipeLineRequest(row.Ingredient!.StockItemId, row.Quantity))
            .ToList());

        MudDialogInstance.Close(DialogResult.Ok(request));
    }

    private void Cancel()
    {
        MudDialogInstance.Cancel();
    }

    private sealed class RecipeRow
    {
        public RecipeIngredientDto? Ingredient { get; set; }

        public decimal Quantity { get; set; }
    }
}