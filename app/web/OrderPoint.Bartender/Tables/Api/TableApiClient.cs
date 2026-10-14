using OrderPoint.Bartender.Tables.Api.Responses;
using OrderPoint.Bartender.Tables.Dtos;
using OrderPoint.Shared.Errors;
using OrderPoint.Shared.Tables.Dtos;

namespace OrderPoint.Bartender.Tables.Api;

// The floor plan the admin set up, and which tables have open orders
internal sealed class TableApiClient(HttpClient httpClient)
{
    internal async Task<IReadOnlyList<AreaDto>> GetFloorPlanAsync(CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.GetAsync("api/floor-plan", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        GetFloorPlanResponse result =
            await response.Content.ReadFromJsonAsync<GetFloorPlanResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(GetFloorPlanResponse)}");

        return result.Data;
    }

    internal async Task<IReadOnlyList<TableStatusDto>> GetTablesStatusAsync(
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.GetAsync("api/tables/status", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        GetTablesStatusResponse result =
            await response.Content.ReadFromJsonAsync<GetTablesStatusResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(GetTablesStatusResponse)}");

        return result.Data;
    }
}