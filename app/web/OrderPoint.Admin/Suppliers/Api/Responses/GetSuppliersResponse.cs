using OrderPoint.Admin.Suppliers.Dtos;
using OrderPoint.Shared.Dtos;

namespace OrderPoint.Admin.Suppliers.Api.Responses;

internal sealed record GetSuppliersResponse(PaginationDto<SupplierDto> Data);