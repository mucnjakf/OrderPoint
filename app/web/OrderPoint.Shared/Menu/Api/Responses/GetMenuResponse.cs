using OrderPoint.Shared.Menu.Dtos;

namespace OrderPoint.Shared.Menu.Api.Responses;

internal sealed record GetMenuResponse(IReadOnlyList<MenuCategoryDto> Data);