using OrderPoint.Admin.Tables.Api.Requests;
using OrderPoint.Admin.Tables.Api.Responses;
using OrderPoint.Shared.Errors;
using OrderPoint.Shared.Tables.Dtos;

namespace OrderPoint.Admin.Tables.Api;

// The floor plan: areas, tables and fixtures
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

    internal async Task<AreaDto> CreateAreaAsync(
        CreateAreaRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient
            .PostAsJsonAsync("api/areas", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        CreateAreaResponse result =
            await response.Content.ReadFromJsonAsync<CreateAreaResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(CreateAreaResponse)}");

        return result.Data;
    }

    internal async Task UpdateAreaAsync(
        Guid id,
        UpdateAreaRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient
            .PutAsJsonAsync($"api/areas/{id}", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }

    internal async Task DeleteAreaAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/areas/{id}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }

    internal async Task<TableDto> CreateTableAsync(
        CreateTableRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient
            .PostAsJsonAsync("api/tables", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        CreateTableResponse result =
            await response.Content.ReadFromJsonAsync<CreateTableResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(CreateTableResponse)}");

        return result.Data;
    }

    internal async Task UpdateTableAsync(
        Guid id,
        UpdateTableRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient
            .PutAsJsonAsync($"api/tables/{id}", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }

    internal async Task MoveTableAsync(
        Guid id,
        MoveTableRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient
            .PutAsJsonAsync($"api/tables/{id}/position", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }

    internal async Task DeleteTableAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/tables/{id}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }

    internal async Task<FixtureDto> CreateFixtureAsync(
        CreateFixtureRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient
            .PostAsJsonAsync("api/fixtures", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        CreateFixtureResponse result =
            await response.Content.ReadFromJsonAsync<CreateFixtureResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(CreateFixtureResponse)}");

        return result.Data;
    }

    internal async Task UpdateFixtureAsync(
        Guid id,
        UpdateFixtureRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient
            .PutAsJsonAsync($"api/fixtures/{id}", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }

    internal async Task MoveFixtureAsync(
        Guid id,
        MoveFixtureRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient
            .PutAsJsonAsync($"api/fixtures/{id}/position", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }

    internal async Task DeleteFixtureAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/fixtures/{id}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }
}