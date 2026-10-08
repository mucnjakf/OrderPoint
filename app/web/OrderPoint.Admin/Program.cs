using Microsoft.AspNetCore.Components.Server.Circuits;
using MudBlazor;
using MudBlazor.Services;
using OrderPoint.Admin;
using OrderPoint.Admin.Auth.Api;
using OrderPoint.Admin.Auth.Services;
using OrderPoint.Admin.Bartenders.Api;
using OrderPoint.Admin.Categories.Api;
using OrderPoint.Admin.Dashboard.Api;
using OrderPoint.Admin.Items.Api;
using OrderPoint.Admin.Orders.Api;
using OrderPoint.Admin.Shared.Services;
using OrderPoint.ServiceDefaults;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services
    .AddHttpClient("OrderPointApi", client =>
    {
        client.BaseAddress = new Uri("https+http://order-point-api");
    })
    .AddHttpMessageHandler<AccessTokenHandler>();

// The auth endpoints are anonymous and called without AccessTokenHandler, which uses them to refresh tokens
builder.Services.AddHttpClient("OrderPointAuthApi", client =>
{
    client.BaseAddress = new Uri("https+http://order-point-api");
});

builder.Services.AddScoped<CircuitServicesAccessor>();
builder.Services.AddScoped<CircuitHandler, ServicesAccessorCircuitHandler>();
builder.Services.AddTransient<AccessTokenHandler>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<AuthApiClient>();
builder.Services.AddScoped<CategoryApiClient>();
builder.Services.AddScoped<ItemApiClient>();
builder.Services.AddScoped<BartenderApiClient>();
builder.Services.AddScoped<OrderApiClient>();
builder.Services.AddScoped<DashboardApiClient>();

builder.Services.AddMudServices(configuration =>
{
    configuration.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    configuration.SnackbarConfiguration.SnackbarVariant = Variant.Outlined;
    configuration.SnackbarConfiguration.ClearAfterNavigation = false;
});

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app
    .MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();