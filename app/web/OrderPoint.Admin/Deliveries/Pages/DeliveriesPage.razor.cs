using System.Globalization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Deliveries.Api;
using OrderPoint.Admin.Deliveries.Dialogs;
using OrderPoint.Admin.Deliveries.Dtos;
using OrderPoint.Admin.Deliveries.Services;
using OrderPoint.Admin.Deliveries.Sorting;
using OrderPoint.Admin.Suppliers.Api;
using OrderPoint.Admin.Suppliers.Dtos;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Services;

namespace OrderPoint.Admin.Deliveries.Pages;

// What suppliers delivered, the latest first, under a header per month
public sealed partial class DeliveriesPage
{
    private const int PageSize = 20;

    private const string ColumnTitleStyle = "color: var(--mud-palette-text-secondary)";

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private DeliveryApiClient DeliveryApiClient { get; set; } = null!;

    [Inject]
    private SupplierApiClient SupplierApiClient { get; set; } = null!;

    [Inject]
    private DeliveryDialogService DeliveryDialogService { get; set; } = null!;

    private PaginationDto<DeliveryDto>? Pagination { get; set; }

    private IReadOnlyList<DeliveryDto> Deliveries { get; set; } = [];

    private string SelectedSortBy { get; set; } = nameof(DeliverySortBy.DeliveredOnDesc);

    private string? SearchQuery { get; set; }

    private SupplierOptionDto? SelectedSupplier { get; set; }

    private MudAutocomplete<SupplierOptionDto> SupplierAutocomplete { get; set; } = null!;

    private bool IsLoading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        await GetDeliveriesAsync(pageNumber: 1);
    }

    private async Task GetDeliveriesAsync(int pageNumber)
    {
        IsLoading = true;

        Pagination = await ApiService.ExecuteAsync(() => DeliveryApiClient.GetDeliveriesAsync(
            pageNumber,
            PageSize,
            SelectedSortBy,
            SearchQuery,
            SelectedSupplier?.Id));

        Deliveries = Pagination?.Items ?? [];

        IsLoading = false;

        StateHasChanged();
    }

    private async Task OnFiltersChangedAsync()
    {
        await GetDeliveriesAsync(pageNumber: 1);
    }

    private async Task OnPageChangedAsync(int pageNumber)
    {
        await GetDeliveriesAsync(pageNumber);
    }

    private async Task<IEnumerable<SupplierOptionDto>> SearchSuppliersAsync(
        string? value,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<SupplierOptionDto>? suppliers = await ApiService.ExecuteAsync(
            () => SupplierApiClient.SearchSuppliersAsync(value, cancellationToken),
            cancellationToken);

        return suppliers ?? [];
    }

    private async Task OnSupplierClearedAsync()
    {
        await SupplierAutocomplete.OpenMenuAsync();
    }

    private async Task RecordDeliveryAsync()
    {
        if (await DeliveryDialogService.RecordAsync())
        {
            await GetDeliveriesAsync(pageNumber: 1);
        }
    }

    private async Task ShowDeliveryDetailsDialogAsync(DeliveryDto delivery)
    {
        var parameters = new DialogParameters<DeliveryDetailsDialog>
        {
            { dialog => dialog.Delivery, delivery }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };

        await DialogService.ShowAsync<DeliveryDetailsDialog>(string.Empty, parameters, options);
    }

    // "October 2026"
    private static string GetMonthText(DateOnly date)
    {
        return date.ToString("MMMM yyyy", CultureInfo.CurrentCulture);
    }

    private static string GetTotalText(IReadOnlyList<DeliveryDto> deliveries)
    {
        return deliveries.Sum(delivery => delivery.Total).ToString("C", CultureInfo.CurrentCulture);
    }
}