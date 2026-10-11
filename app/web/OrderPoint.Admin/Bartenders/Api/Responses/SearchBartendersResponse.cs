using OrderPoint.Admin.Bartenders.Dtos;

namespace OrderPoint.Admin.Bartenders.Api.Responses;

internal sealed record SearchBartendersResponse(IReadOnlyList<BartenderDto> Data);