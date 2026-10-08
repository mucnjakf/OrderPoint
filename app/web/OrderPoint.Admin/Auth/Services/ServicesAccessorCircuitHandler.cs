using Microsoft.AspNetCore.Components.Server.Circuits;

namespace OrderPoint.Admin.Auth.Services;

// Makes the circuit's services available to CircuitServicesAccessor while the circuit handles browser activity
internal sealed class ServicesAccessorCircuitHandler(
    IServiceProvider serviceProvider,
    CircuitServicesAccessor circuitServicesAccessor)
    : CircuitHandler
{
    public override Func<CircuitInboundActivityContext, Task> CreateInboundActivityHandler(
        Func<CircuitInboundActivityContext, Task> next)
        => async context =>
        {
            circuitServicesAccessor.Services = serviceProvider;
            await next(context);
            circuitServicesAccessor.Services = null;
        };
}