using OrderPoint.Customer.Menu.Api.Responses;
using OrderPoint.Customer.Menu.Dtos;
using OrderPoint.Shared.Errors;

namespace OrderPoint.Customer.Menu.Api;

internal sealed class MenuApiClient(HttpClient httpClient)
{
    internal async Task<IReadOnlyList<MenuCategoryDto>> GetMenuAsync(CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.GetAsync("api/menu", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        GetMenuResponse result =
            await response.Content.ReadFromJsonAsync<GetMenuResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(GetMenuResponse)}");

        return result.Data;
    }
}