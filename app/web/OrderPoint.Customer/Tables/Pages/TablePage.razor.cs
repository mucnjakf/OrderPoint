using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using OrderPoint.Customer.Tables.Services;

namespace OrderPoint.Customer.Tables.Pages;

// Where a table's QR code points: remembers the table and opens the menu
public sealed partial class TablePage
{
    [Inject]
    private TableService TableService { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public string TableCode { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        string tableCode = TableCode.Trim().ToUpperInvariant();

        // A mistyped or odd URL keeps the table the guest had
        if (tableCode.Length <= TableService.MaxTableCodeLength && TableCodeRegex().IsMatch(tableCode))
        {
            await TableService.SetAsync(tableCode);
        }

        NavigationManager.NavigateTo("/", replace: true);
    }

    [GeneratedRegex("^[A-Z0-9-]+$")]
    private static partial Regex TableCodeRegex();
}