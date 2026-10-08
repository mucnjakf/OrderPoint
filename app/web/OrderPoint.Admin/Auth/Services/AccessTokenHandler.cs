using System.Net.Http.Headers;

namespace OrderPoint.Admin.Auth.Services;

// IHttpClientFactory creates handlers in its own DI scope, so the circuit's AuthService is reached through
// CircuitServicesAccessor
internal sealed class AccessTokenHandler(CircuitServicesAccessor circuitServicesAccessor) : DelegatingHandler
{
    private const string BearerScheme = "Bearer";

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        IServiceProvider circuitServices = circuitServicesAccessor.Services
            ?? throw new InvalidOperationException("The API can only be called from a Blazor circuit");

        var authService = circuitServices.GetRequiredService<AuthService>();

        string accessToken = await authService.GetAccessTokenAsync(cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue(BearerScheme, accessToken);

        return await base.SendAsync(request, cancellationToken);
    }
}