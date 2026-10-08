namespace OrderPoint.Admin.Auth.Services;

// Gives code outside the circuit's DI scope (HttpClient handlers) access to the current circuit's services.
// See "Access server-side Blazor services from a different DI scope" in the ASP.NET Core docs.
internal sealed class CircuitServicesAccessor
{
    private static readonly AsyncLocal<IServiceProvider?> CircuitServices = new();

    internal IServiceProvider? Services
    {
        get => CircuitServices.Value;
        set => CircuitServices.Value = value;
    }
}