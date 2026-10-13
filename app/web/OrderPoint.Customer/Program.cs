using System.Globalization;
using MudBlazor;
using MudBlazor.Services;
using OrderPoint.Customer;
using OrderPoint.Customer.Cart.Services;
using OrderPoint.Customer.Menu.Api;
using OrderPoint.Customer.Menu.Services;
using OrderPoint.Customer.Orders.Api;
using OrderPoint.Customer.Orders.Services;
using OrderPoint.Customer.Settings.Services;
using OrderPoint.Customer.Tables.Services;
using OrderPoint.ServiceDefaults;
using OrderPoint.Shared.Extensions;
using OrderPoint.Shared.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<TimeZoneService>();
builder.Services.AddScoped<TableService>();
builder.Services.AddScoped<AppearanceService>();
builder.Services.AddScoped<MenuService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<MyOrdersService>();
builder.Services.AddScoped<OrderTrackingHubService>();

// Guests are anonymous, so no client sends a token
builder.Services.AddHttpClient<MenuApiClient>(ConfigureApiClient);
builder.Services.AddHttpClient<OrderApiClient>(ConfigureApiClient);

// MudBlazor's default is 5000
const int SnackbarVisibleMilliseconds = 2000;

// At the top, so snackbars never cover the bottom navigation
builder.Services.AddMudServices(configuration =>
{
    configuration.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopCenter;
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