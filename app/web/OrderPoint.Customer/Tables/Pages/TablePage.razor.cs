using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using OrderPoint.Customer.Tables.Api;
using OrderPoint.Customer.Tables.Dtos;
using OrderPoint.Customer.Tables.Services;
using OrderPoint.Shared.Services;
using OrderPoint.Shared.Tables.Enumerations;

namespace OrderPoint.Customer.Tables.Pages;

// Where a table's QR code points: checks the table, remembers it and opens the menu
public sealed partial class TablePage
{
    [Inject]
    private TableService TableService { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private TableApiClient TableApiClient { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public string TableCode { get; set; } = null!;

    // Unknown, closed or an odd URL: the guest keeps the table they had, if any
    private bool IsUnavailable { get; set; }

    protected override async Task OnInitializedAsync()
    {
        string tableCode = TableCode.Trim().ToUpperInvariant();

        if (tableCode.Length > TableService.MaxTableCodeLength || !TableCodeRegex().IsMatch(tableCode))
        {
            IsUnavailable = true;

            return;
        }

        GuestTableDto? table = await ApiService.ExecuteAsync(() => TableApiClient.GetTableByCodeAsync(tableCode));

        if (table?.Status is not TableStatus.Active)
        {
            IsUnavailable = true;

            return;
        }

        await TableService.SetAsync(table.Code, table.AreaName);

        NavigationManager.NavigateTo("/", replace: true);
    }

    [GeneratedRegex("^[A-Z0-9-]+$")]
    private static partial Regex TableCodeRegex();
}