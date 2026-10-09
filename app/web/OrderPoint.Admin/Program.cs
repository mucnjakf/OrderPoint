using System.Globalization;
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
using OrderPoint.Admin.Shared.Extensions;
using OrderPoint.Admin.Shared.Services;
using OrderPoint.ServiceDefaults;

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

// The auth endpoints are anonymous and called without AccessTokenHandler, which uses them to refresh tokens
builder.Services.AddHttpClient<AuthApiClient>(ConfigureApiClient);
builder.Services.AddHttpClient<CategoryApiClient>(ConfigureApiClient).AddHttpMessageHandler<AccessTokenHandler>();
builder.Services.AddHttpClient<ItemApiClient>(ConfigureApiClient).AddHttpMessageHandler<AccessTokenHandler>();
builder.Services.AddHttpClient<BartenderApiClient>(ConfigureApiClient).AddHttpMessageHandler<AccessTokenHandler>();
builder.Services.AddHttpClient<OrderApiClient>(ConfigureApiClient).AddHttpMessageHandler<AccessTokenHandler>();
builder.Services.AddHttpClient<DashboardApiClient>(ConfigureApiClient).AddHttpMessageHandler<AccessTokenHandler>();

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