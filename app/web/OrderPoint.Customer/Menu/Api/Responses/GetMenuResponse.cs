using OrderPoint.Customer.Menu.Dtos;

namespace OrderPoint.Customer.Menu.Api.Responses;

internal sealed record GetMenuResponse(IReadOnlyList<MenuCategoryDto> Data);