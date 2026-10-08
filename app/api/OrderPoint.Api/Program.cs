using System.Reflection;
using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Exceptions;
using OrderPoint.Api.Extensions;
using OrderPoint.Application;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Infrastructure;
using OrderPoint.Infrastructure.Identity;
using OrderPoint.ServiceDefaults;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Aspire
builder.AddServiceDefaults();

// API docs
builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

// Authentication
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services
    .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((options, jwtOptions) =>
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwtOptions.Value.Issuer,
            ValidAudience = jwtOptions.Value.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Value.SigningKey)),
            ClockSkew = TimeSpan.Zero
        });

// Authorization
builder.Services
    .AddAuthorizationBuilder()
    .AddPolicy(AuthorizationPolicies.Admin, policy => policy.RequireRole(nameof(UserRole.Admin)));

// Cors
builder.Services.AddCors(options =>
    options.AddPolicy("AllowAll", configure
        => configure
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin()));

// Error handling
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<RequestValidationExceptionHandler>();
builder.Services.AddExceptionHandler<BadHttpRequestExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Minimal API
builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());
builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>(includeInternalTypes: true);

// Storage
builder.AddAzureBlobContainerClient("images");

// Modules
builder.Services.AddApplicationModule(builder.Configuration);
builder.Services.AddInfrastructureModule(builder.Configuration);

// ---------------------------------------------------------------------------------------------------------------------

WebApplication app = builder.Build();

// Database
app.ApplyMigrations();
await app.SeedAdminAsync();

// Storage
app.CreateImageContainer();

// Error handling
app.UseExceptionHandler();

// HTTP
app.UseHttpsRedirection();

// Cors
app.UseCors("AllowAll");

// Authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Minimal API
app.MapEndpoints();

// API docs
app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options
        .WithTitle("OrderPoint API")
        .WithClassicLayout()
        .WithTheme(ScalarTheme.Alternate)
        .ExpandAllTags()
        .SortOperationsByMethod();

    options.HideClientButton = true;
});

// Aspire
app.MapDefaultEndpoints();

app.Run();