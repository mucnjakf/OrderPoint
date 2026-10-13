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
        string tableCode = TableCode.Trim();

        if (tableCode.Length is > 0 and <= TableService.MaxTableCodeLength)
        {
            await TableService.SetAsync(tableCode);
        }

        NavigationManager.NavigateTo("/", replace: true);
    }
}