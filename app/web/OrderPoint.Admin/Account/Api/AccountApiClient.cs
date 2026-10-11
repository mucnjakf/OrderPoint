using System.Net.Http.Headers;
using OrderPoint.Admin.Account.Api.Requests;
using OrderPoint.Admin.Account.Api.Responses;
using OrderPoint.Admin.Account.Dtos;
using OrderPoint.Admin.Auth.Dtos;
using OrderPoint.Admin.Shared.Dtos;
using OrderPoint.Admin.Shared.Errors;

namespace OrderPoint.Admin.Account.Api;

internal sealed class AccountApiClient(HttpClient httpClient)
{
    private const string ImageFormFieldName = "image";

    internal async Task<AccountDto> GetAccountAsync(CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.GetAsync("api/account", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        GetAccountResponse result =
            await response.Content.ReadFromJsonAsync<GetAccountResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(GetAccountResponse)}");

        return result.Data;
    }

    internal async Task UpdateAccountImageAsync(ImageFileDto image, CancellationToken cancellationToken = default)
    {
        using var fileContent = new ByteArrayContent(image.Content);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(image.ContentType);

        using var formContent = new MultipartFormDataContent();
        formContent.Add(fileContent, ImageFormFieldName, image.FileName);

        HttpResponseMessage response = await httpClient.PutAsync("api/account/image", formContent, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }

    internal async Task DeleteAccountImageAsync(CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync("api/account/image", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }

    internal async Task<AuthTokensDto> ChangePasswordAsync(
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient
            .PutAsJsonAsync("api/account/password", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        ChangePasswordResponse result =
            await response.Content.ReadFromJsonAsync<ChangePasswordResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(ChangePasswordResponse)}");

        return result.Data;
    }

    internal async Task SignOutEverywhereAsync(CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient
            .PostAsync("api/account/sign-out-everywhere", content: null, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }
}