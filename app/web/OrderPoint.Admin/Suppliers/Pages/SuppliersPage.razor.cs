using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Suppliers.Api;
using OrderPoint.Admin.Suppliers.Api.Requests;
using OrderPoint.Admin.Suppliers.Dialogs;
using OrderPoint.Admin.Suppliers.Dtos;
using OrderPoint.Admin.Suppliers.Enumerations;
using OrderPoint.Admin.Suppliers.Sorting;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Services;

namespace OrderPoint.Admin.Suppliers.Pages;

// Who the bar buys from, as contact cards
public sealed partial class SuppliersPage
{
    // 4 cards per row, 4 rows
    private const int PageSize = 16;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private SupplierApiClient SupplierApiClient { get; set; } = null!;

    private PaginationDto<SupplierDto>? Pagination { get; set; }

    private IReadOnlyList<SupplierDto> Suppliers { get; set; } = [];

    private string SelectedSortBy { get; set; } = nameof(SupplierSortBy.NameAsc);

    private string? SearchQuery { get; set; }

    private SupplierStatus? SelectedStatus { get; set; }

    private bool IsLoading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        await GetSuppliersAsync(pageNumber: 1);
    }

    private async Task GetSuppliersAsync(int pageNumber)
    {
        IsLoading = true;

        Pagination = await ApiService.ExecuteAsync(() => SupplierApiClient.GetSuppliersAsync(
            pageNumber,
            PageSize,
            SelectedSortBy,
            SearchQuery,
            SelectedStatus));

        Suppliers = Pagination?.Items ?? [];

        IsLoading = false;

        StateHasChanged();
    }

    private async Task OnFiltersChangedAsync()
    {
        await GetSuppliersAsync(pageNumber: 1);
    }

    private async Task OnPageChangedAsync(int pageNumber)
    {
        await GetSuppliersAsync(pageNumber);
    }

    private async Task ShowSupplierDetailsDialogAsync(SupplierDto supplier)
    {
        var parameters = new DialogParameters<SupplierDetailsDialog>
        {
            { dialog => dialog.Supplier, supplier }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };

        await DialogService.ShowAsync<SupplierDetailsDialog>(string.Empty, parameters, options);
    }

    private async Task ShowCreateSupplierDialogAsync()
    {
        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<CreateSupplierDialog>(string.Empty, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        var request = (dialogResult.Data as CreateSupplierRequest)!;

        bool isSuccess = await ApiService.ExecuteAsync(
            () => SupplierApiClient.CreateSupplierAsync(request),
            $"Supplier {request.Name} created successfully");

        if (isSuccess)
        {
            await GetSuppliersAsync(pageNumber: 1);
        }
    }

    private async Task ShowUpdateSupplierDialogAsync(SupplierDto supplier)
    {
        var parameters = new DialogParameters<UpdateSupplierDialog>
        {
            { dialog => dialog.Supplier, supplier }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<UpdateSupplierDialog>(string.Empty, parameters, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        var request = (dialogResult.Data as UpdateSupplierRequest)!;

        bool isSuccess = await ApiService.ExecuteAsync(
            () => SupplierApiClient.UpdateSupplierAsync(supplier.Id, request),
            $"Supplier {request.Name} edited successfully");

        if (isSuccess)
        {
            await GetSuppliersAsync(Pagination?.PageNumber ?? 1);
        }
    }

    private async Task ShowDeleteSupplierDialogAsync(SupplierDto supplier)
    {
        var parameters = new DialogParameters<DeleteSupplierDialog>
        {
            { dialog => dialog.Supplier, supplier }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<DeleteSupplierDialog>(string.Empty, parameters, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        bool isSuccess = await ApiService.ExecuteAsync(
            () => SupplierApiClient.DeleteSupplierAsync(supplier.Id),
            $"Supplier {supplier.Name} deleted successfully");

        if (isSuccess)
        {
            await GetSuppliersAsync(pageNumber: 1);
        }
    }
}