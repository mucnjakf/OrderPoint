using OrderPoint.Customer.Tables.Api.Requests;
using OrderPoint.Customer.Tables.Api.Responses;
using OrderPoint.Customer.Tables.Dtos;
using OrderPoint.Shared.Errors;
using OrderPoint.Shared.Tables.Dtos;

namespace OrderPoint.Customer.Tables.Api;

internal sealed class TableApiClient(HttpClient httpClient)
{
    private const string TableNotFoundCode = "Table.NotFound";

    // Null for a code no table has, e.g. an old or mistyped QR code
    internal async Task<GuestTableDto?> GetTableByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.GetAsync(
            $"api/tables/by-code/{Uri.EscapeDataString(code)}",
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            try
            {
                await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
            }
            catch (ApiException exception) when (exception.HasErrorCode(TableNotFoundCode))
            {
                return null;
            }
        }

        GetTableByCodeResponse result =
            await response.Content.ReadFromJsonAsync<GetTableByCodeResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(GetTableByCodeResponse)}");

        return result.Data;
    }

    // The table's open requests: at most one of each type
    internal async Task<IReadOnlyList<TableRequestDto>> GetTableRequestsAsync(
        string tableCode,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.GetAsync(
            $"api/tables/by-code/{Uri.EscapeDataString(tableCode)}/requests",
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        GetTableRequestsResponse result =
            await response.Content.ReadFromJsonAsync<GetTableRequestsResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(GetTableRequestsResponse)}");

        return result.Data;
    }

    // Asking again while a request of that type is open returns the open one
    internal async Task<TableRequestDto> CreateTableRequestAsync(
        string tableCode,
        CreateTableRequestRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            $"api/tables/by-code/{Uri.EscapeDataString(tableCode)}/requests",
            request,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        CreateTableRequestResponse result =
            await response.Content.ReadFromJsonAsync<CreateTableRequestResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(CreateTableRequestResponse)}");

        return result.Data;
    }
}