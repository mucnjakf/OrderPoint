using OrderPoint.Bartender.Tables.Api.Responses;
using OrderPoint.Shared.Errors;
using OrderPoint.Shared.Tables.Dtos;

namespace OrderPoint.Bartender.Tables.Api;

// The tables the admin set up, and the guests who call a bartender
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

    // Every open request of every table, oldest first
    internal async Task<IReadOnlyList<TableRequestDto>> GetTableRequestsAsync(
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.GetAsync("api/table-requests", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        GetTableRequestsResponse result =
            await response.Content.ReadFromJsonAsync<GetTableRequestsResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(GetTableRequestsResponse)}");

        return result.Data;
    }

    internal async Task CompleteTableRequestAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient
            .PutAsync($"api/table-requests/{id}/done", content: null, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }
}