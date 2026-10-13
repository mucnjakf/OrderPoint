using System.Globalization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using MudBlazor;
using MudBlazor.Services;
using OrderPoint.Bartender;
using OrderPoint.Bartender.Orders.Api;
using OrderPoint.Bartender.Orders.Services;
using OrderPoint.ServiceDefaults;
using OrderPoint.Shared.Account.Api;
using OrderPoint.Shared.Account.Services;
using OrderPoint.Shared.Auth.Api;
using OrderPoint.Shared.Auth.Services;
using OrderPoint.Shared.Extensions;
using OrderPoint.Shared.Orders.Services;
using OrderPoint.Shared.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<CircuitServicesAccessor>();
builder.Services.AddScoped<CircuitHandler, ServicesAccessorCircuitHandler>();
builder.Services.AddTransient<AccessTokenHandler>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<TimeZoneService>();
builder.Services.AddScoped<DarkModeService>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<OrdersHubService>();
builder.Services.AddScoped<OrdersTabState>();

// The auth endpoints are anonymous and called without AccessTokenHandler, which uses them to refresh tokens
builder.Services.AddHttpClient<AuthApiClient>(ConfigureApiClient);
builder.Services.AddHttpClient<AccountApiClient>(ConfigureApiClient).AddHttpMessageHandler<AccessTokenHandler>();
builder.Services.AddHttpClient<OrderApiClient>(ConfigureApiClient).AddHttpMessageHandler<AccessTokenHandler>();

// At the top, so snackbars never cover the bottom navigation
builder.Services.AddMudServices(configuration =>
{
    configuration.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopCenter;
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

app.UseRequestLocalization(options =>
{
    CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);

    options.SetDefaultCulture("en-US");
    options.SupportedCultures = cultures;
    options.SupportedUICultures = cultures;
});

app.Use(async (context, next) =>
{
    CultureInfo.CurrentCulture = CultureInfo.CurrentCulture.ToDisplayCulture();

    await next(context);
});

app.UseAntiforgery();

app.MapStaticAssets();

app
    .MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();

static void ConfigureApiClient(HttpClient client)
{
    client.BaseAddress = new Uri("https+http://order-point-api");
}