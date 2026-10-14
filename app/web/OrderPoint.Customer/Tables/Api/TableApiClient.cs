using OrderPoint.Customer.Tables.Api.Responses;
using OrderPoint.Customer.Tables.Dtos;
using OrderPoint.Shared.Errors;

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
}