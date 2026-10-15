using System.Globalization;
using Microsoft.AspNetCore.Components;
using OrderPoint.Bartender.Menu.Services;
using OrderPoint.Bartender.Orders.Api;
using OrderPoint.Bartender.Orders.Api.Requests;
using OrderPoint.Bartender.Orders.Dtos;
using OrderPoint.Bartender.Orders.Services;
using OrderPoint.Bartender.Shared.Extensions;
using OrderPoint.Shared.Errors;
using OrderPoint.Shared.Menu.Dtos;
using OrderPoint.Shared.Services;

namespace OrderPoint.Bartender.Orders.Pages;

// The lines with today's prices, a note, a name at the counter; sent unpaid, the guest pays at the register
public sealed partial class ReviewOrderPage : IDisposable
{
    // As long as the API accepts
    private const int NoteMaxLength = 200;

    private const int CustomerNameMaxLength = 30;

    private const string ItemIsUnavailableCode = "Order.ItemIsUnavailable";

    private const string ItemNotFoundCode = "Item.NotFound";

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private MenuService MenuService { get; set; } = null!;

    [Inject]
    private OrderApiClient OrderApiClient { get; set; } = null!;

    [Inject]
    private OrderDraftService OrderDraftService { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    // Null at the counter
    [Parameter]
    public string? TableCode { get; set; }

    private Dictionary<Guid, MenuItemDto> MenuItems { get; set; } = [];

    private bool IsLoading { get; set; } = true;

    private bool IsSending { get; set; }

    private OrderDraft Draft => OrderDraftService.Get(TableCode);

    private string PlaceText => TableCode.ToPlaceText();

    private string OrderPadUri => TableCode.ToOrderPadUri();

    private IEnumerable<(OrderDraftLineDto Line, MenuItemDto? Item)> Rows => Draft.Lines
        .Select(line => (line, MenuItems.GetValueOrDefault(line.ItemId)));

    private bool HasUnavailableLines => Rows.Any(row => row.Item is null);

    private decimal Total => Rows
        .Where(row => row.Item is not null)
        .Sum(row => row.Item!.Price * row.Line.Quantity);

    private bool CanSend => !IsSending && Draft.Lines.Count != 0 && !HasUnavailableLines;

    protected override async Task OnInitializedAsync()
    {
        MenuService.Changed += OnMenuChanged;

        await GetMenuItemsAsync();
    }

    public void Dispose()
    {
        MenuService.Changed -= OnMenuChanged;
    }

    private void OnMenuChanged()
    {
        _ = InvokeAsync(() => GetMenuItemsAsync(isLiveUpdate: true));
    }

    // New prices, and lines whose item just left the menu; a failed reload keeps what is shown
    private async Task GetMenuItemsAsync(bool isLiveUpdate = false)
    {
        IReadOnlyList<MenuCategoryDto>? menu = await MenuService.GetMenuAsync();

        if (menu is not null || !isLiveUpdate)
        {
            MenuItems = (menu ?? []).SelectMany(category => category.Items).ToDictionary(item => item.Id);
        }

        IsLoading = false;

        StateHasChanged();
    }

    private static string GetLineText(MenuItemDto item, OrderDraftLineDto line)
    {
        string price = item.Price.ToString("C", CultureInfo.CurrentCulture);
        string lineTotal = (item.Price * line.Quantity).ToString("C", CultureInfo.CurrentCulture);

        return $"{price} each · {lineTotal}";
    }

    private void SetQuantity(OrderDraftLineDto line, int quantity)
    {
        Draft.SetQuantity(line.ItemId, line.Name, quantity);
    }

    private async Task SendAsync()
    {
        IsSending = true;

        bool isSuccess = await ApiService.ExecuteAsync(SendOrderAsync, $"Order for {PlaceText} sent to the bar");

        IsSending = false;

        if (!isSuccess)
        {
            return;
        }

        OrderDraftService.Remove(TableCode);

        // The order shows up there, and follows the bar live
        NavigationManager.NavigateTo("/orders");
    }

    private async Task SendOrderAsync()
    {
        CreateBartenderOrderRequest request = new(
            TableCode,
            TableCode is null && !string.IsNullOrWhiteSpace(Draft.CustomerName) ? Draft.CustomerName : null,
            string.IsNullOrWhiteSpace(Draft.Note) ? null : Draft.Note,
            Draft.Lines.Select(line => new CreateOrderItemRequest(line.ItemId, line.Quantity)).ToList());

        try
        {
            await OrderApiClient.CreateBartenderOrderAsync(request);
        }
        catch (ApiException exception)
            when (exception.HasErrorCode(ItemIsUnavailableCode) || exception.HasErrorCode(ItemNotFoundCode))
        {
            // The menu changed since it was loaded: reload it, so the gone lines are marked
            MenuService.Reset();

            await GetMenuItemsAsync(isLiveUpdate: true);

            throw;
        }
    }
}