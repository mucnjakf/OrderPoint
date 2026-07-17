using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using OrderPoint.Admin.Categories.Api;
using OrderPoint.Admin.Categories.Api.Requests;

namespace OrderPoint.Admin.Categories.Pages.Create;

public sealed partial class CreateCategoryPage
{
    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    private CategoryApiClient CategoryApiClient { get; set; } = null!;

    private List<BreadcrumbItem> Breadcrumbs { get; set; } =
    [
        new("Dashboard", href: "/", icon: Icons.Material.Filled.Dashboard),
        new("Categories", href: "/categories", icon: Icons.Material.Filled.Category),
        new("New", href: null, disabled: true, icon: Icons.Material.Filled.Add)
    ];

    private CreateCategoryRequest Request { get; set; } = new();

    private async Task OnValidSubmitAsync(EditContext editContext)
    {
        StateHasChanged();

        await CategoryApiClient.CreateCategoryAsync(Request);

        Snackbar.Add($"Category {Request.Name} created successfully", Severity.Success);

        NavigationManager.NavigateTo("/categories");
    }
}