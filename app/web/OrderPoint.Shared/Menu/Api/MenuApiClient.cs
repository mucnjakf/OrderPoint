using System.Net.Http.Json;
using OrderPoint.Shared.Errors;
using OrderPoint.Shared.Menu.Api.Responses;
using OrderPoint.Shared.Menu.Dtos;

namespace OrderPoint.Shared.Menu.Api;

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