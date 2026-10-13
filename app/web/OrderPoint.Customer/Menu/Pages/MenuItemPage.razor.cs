using Microsoft.AspNetCore.Components;
using OrderPoint.Customer.Menu.Dtos;
using OrderPoint.Customer.Menu.Services;

namespace OrderPoint.Customer.Menu.Pages;

// Under its category's route, so the back arrow returns to that category
public sealed partial class MenuItemPage
{
    [Inject]
    private MenuService MenuService { get; set; } = null!;

    [Parameter]
    public Guid CategoryId { get; set; }

    [Parameter]
    public Guid ItemId { get; set; }

    private MenuCategoryDto? Category { get; set; }

    private MenuItemDto? Item { get; set; }

    private bool IsLoading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        IReadOnlyList<MenuCategoryDto> menu = await MenuService.GetMenuAsync() ?? [];

        Category = menu.FirstOrDefault(category => category.Id == CategoryId);
        Item = Category?.Items.FirstOrDefault(item => item.Id == ItemId);

        IsLoading = false;
    }
}