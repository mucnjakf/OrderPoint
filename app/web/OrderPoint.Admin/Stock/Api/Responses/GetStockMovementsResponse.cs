using OrderPoint.Admin.Stock.Dtos;
using OrderPoint.Shared.Dtos;

namespace OrderPoint.Admin.Stock.Api.Responses;

internal sealed record GetStockMovementsResponse(PaginationDto<StockMovementDto> Data);