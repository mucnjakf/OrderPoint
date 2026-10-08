using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Bartenders.Api;
using OrderPoint.Admin.Bartenders.Api.Requests;
using OrderPoint.Admin.Bartenders.Dialogs;
using OrderPoint.Admin.Bartenders.Dtos;
using OrderPoint.Admin.Bartenders.Enumerations;
using OrderPoint.Admin.Bartenders.Sorting;
using OrderPoint.Admin.Shared.Dtos;
using OrderPoint.Admin.Shared.Services;

namespace OrderPoint.Admin.Bartenders.Pages;

public sealed partial class BartendersPage
{
    private const int PageSize = 10;

    private const int ActiveTeamAvatarsCount = 5;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private BartenderApiClient BartenderApiClient { get; set; } = null!;

    private List<BreadcrumbItem> Breadcrumbs { get; set; } =
    [
        new("Dashboard", href: "/", icon: Icons.Material.Filled.Dashboard),
        new("Bartenders", href: null, disabled: true, icon: Icons.Material.Filled.People)
    ];

    private IReadOnlyList<BartenderDto> ActiveBartenders { get; set; } = [];

    private int ActiveCount { get; set; }

    private int InactiveCount { get; set; }

    private BartenderDto? NewestBartender { get; set; }

    private bool IsLoadingActiveTeam { get; set; } = true;

    private bool IsLoadingInactiveCount { get; set; } = true;

    private bool IsLoadingNewestBartender { get; set; } = true;

    private PaginationDto<BartenderDto>? Pagination { get; set; }

    private IReadOnlyList<BartenderDto> Bartenders { get; set; } = [];

    private string SelectedSortBy { get; set; } = nameof(BartenderSortBy.CreatedAtUtcDesc);

    private string? SearchQuery { get; set; }

    private BartenderStatus? SelectedStatus { get; set; }

    private bool IsLoading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        await Task.WhenAll(
            GetTeamStripAsync(),
            GetBartendersAsync(pageNumber: 1));
    }

    private async Task GetTeamStripAsync()
    {
        await Task.WhenAll(
            GetActiveTeamAsync(),
            GetInactiveCountAsync(),
            GetNewestBartenderAsync());
    }

    private async Task GetActiveTeamAsync()
    {
        IsLoadingActiveTeam = true;

        PaginationDto<BartenderDto>? pagination = await ApiService.ExecuteAsync(
            () => BartenderApiClient.GetBartendersAsync(
                pageNumber: 1,
                ActiveTeamAvatarsCount,
                nameof(BartenderSortBy.NameAsc),
                status: BartenderStatus.Active));

        ActiveBartenders = pagination?.Items ?? [];
        ActiveCount = pagination?.TotalCount ?? 0;

        IsLoadingActiveTeam = false;

        StateHasChanged();
    }

    private async Task GetInactiveCountAsync()
    {
        IsLoadingInactiveCount = true;

        PaginationDto<BartenderDto>? pagination = await ApiService.ExecuteAsync(
            () => BartenderApiClient.GetBartendersAsync(
                pageNumber: 1,
                pageSize: 1,
                nameof(BartenderSortBy.CreatedAtUtcDesc),
                status: BartenderStatus.Inactive));

        InactiveCount = pagination?.TotalCount ?? 0;

        IsLoadingInactiveCount = false;

        StateHasChanged();
    }

    private async Task GetNewestBartenderAsync()
    {
        IsLoadingNewestBartender = true;

        PaginationDto<BartenderDto>? pagination = await ApiService.ExecuteAsync(
            () => BartenderApiClient.GetBartendersAsync(
                pageNumber: 1,
                pageSize: 1,
                nameof(BartenderSortBy.CreatedAtUtcDesc)));

        NewestBartender = pagination?.Items.FirstOrDefault();

        IsLoadingNewestBartender = false;

        StateHasChanged();
    }

    private async Task GetBartendersAsync(int pageNumber)
    {
        IsLoading = true;

        Pagination = await ApiService.ExecuteAsync(() => BartenderApiClient.GetBartendersAsync(
            pageNumber,
            PageSize,
            SelectedSortBy,
            SearchQuery,
            SelectedStatus));

        Bartenders = Pagination?.Items ?? [];

        IsLoading = false;

        StateHasChanged();
    }

    private async Task OnSearchChangedAsync()
    {
        await GetBartendersAsync(pageNumber: 1);
    }

    private async Task OnSortChangedAsync()
    {
        await GetBartendersAsync(pageNumber: 1);
    }

    private async Task OnStatusChangedAsync()
    {
        await GetBartendersAsync(pageNumber: 1);
    }

    private async Task OnPageChangedAsync(int pageNumber)
    {
        await GetBartendersAsync(pageNumber);
    }

    private async Task ShowCreateBartenderDialogAsync()
    {
        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<CreateBartenderDialog>(string.Empty, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        var request = (dialogResult.Data as CreateBartenderRequest)!;

        bool isSuccess = await ApiService.ExecuteAsync(
            () => BartenderApiClient.CreateBartenderAsync(request),
            $"Bartender {request.FirstName} {request.LastName} created successfully");

        if (isSuccess)
        {
            await Task.WhenAll(
                GetTeamStripAsync(),
                GetBartendersAsync(pageNumber: 1));
        }
    }

    private async Task ShowUpdateBartenderDialogAsync(BartenderDto bartender)
    {
        var parameters = new DialogParameters<UpdateBartenderDialog>
        {
            { dialog => dialog.Bartender, bartender }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<UpdateBartenderDialog>(string.Empty, parameters, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        var request = (dialogResult.Data as UpdateBartenderRequest)!;

        bool isSuccess = await ApiService.ExecuteAsync(
            () => BartenderApiClient.UpdateBartenderAsync(bartender.Id, request),
            $"Bartender {request.FirstName} {request.LastName} edited successfully");

        if (isSuccess)
        {
            await Task.WhenAll(
                GetTeamStripAsync(),
                GetBartendersAsync(pageNumber: 1));
        }
    }

    private async Task ShowDeleteBartenderDialogAsync(BartenderDto bartender)
    {
        string bartenderName = $"{bartender.FirstName} {bartender.LastName}";

        var parameters = new DialogParameters<DeleteBartenderDialog>
        {
            { dialog => dialog.BartenderName, bartenderName }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<DeleteBartenderDialog>(string.Empty, parameters, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        bool isSuccess = await ApiService.ExecuteAsync(
            () => BartenderApiClient.DeleteBartenderAsync(bartender.Id),
            $"Bartender {bartenderName} deleted successfully");

        if (isSuccess)
        {
            await Task.WhenAll(
                GetTeamStripAsync(),
                GetBartendersAsync(pageNumber: 1));
        }
    }
}