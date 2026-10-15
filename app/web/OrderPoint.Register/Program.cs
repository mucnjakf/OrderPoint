using System.Globalization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using MudBlazor;
using MudBlazor.Services;
using OrderPoint.Register;
using OrderPoint.Register.Menu.Services;
using OrderPoint.Register.Orders.Api;
using OrderPoint.Register.Orders.Services;
using OrderPoint.Register.Today.Api;
using OrderPoint.Register.Sale.Services;
using OrderPoint.ServiceDefaults;
using OrderPoint.Shared.Account.Api;
using OrderPoint.Shared.Account.Services;
using OrderPoint.Shared.Auth.Api;
using OrderPoint.Shared.Auth.Services;
using OrderPoint.Shared.Extensions;
using OrderPoint.Shared.Menu.Api;
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
builder.Services.AddScoped<MenuService>();
builder.Services.AddScoped<SaleService>();
builder.Services.AddScoped<PaymentDialogService>();

// The auth endpoints are anonymous and called without AccessTokenHandler, which uses them to refresh tokens
builder.Services.AddHttpClient<AuthApiClient>(ConfigureApiClient);
builder.Services.AddHttpClient<AccountApiClient>(ConfigureApiClient).AddHttpMessageHandler<AccessTokenHandler>();
builder.Services.AddHttpClient<OrderApiClient>(ConfigureApiClient).AddHttpMessageHandler<AccessTokenHandler>();
builder.Services.AddHttpClient<RegisterDayApiClient>(ConfigureApiClient).AddHttpMessageHandler<AccessTokenHandler>();
builder.Services.AddHttpClient<MenuApiClient>(ConfigureApiClient).AddHttpMessageHandler<AccessTokenHandler>();

// MudBlazor's default is 5000
const int SnackbarVisibleMilliseconds = 2000;

builder.Services.AddMudServices(configuration =>
{
    configuration.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    configuration.SnackbarConfiguration.VisibleStateDuration = SnackbarVisibleMilliseconds;
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