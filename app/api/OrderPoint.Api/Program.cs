using System.Reflection;
using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Exceptions;
using OrderPoint.Api.Extensions;
using OrderPoint.Api.OpenApi;
using OrderPoint.Application;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Infrastructure;
using OrderPoint.Infrastructure.EfCore;
using OrderPoint.Infrastructure.Identity;
using OrderPoint.ServiceDefaults;
using StackExchange.Redis;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Aspire
builder.AddServiceDefaults();

// API docs
builder.Services.AddApiDocumentation();

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
const string corsPolicyName = "AllowedOrigins";

string[] allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
    options.AddPolicy(corsPolicyName, configure
        => configure
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .WithMethods(HttpMethods.Get, HttpMethods.Post, HttpMethods.Put, HttpMethods.Delete)));

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

// Cache: Redis behind HybridCache. While Redis is down, commands fail fast instead of waiting for a timeout, reads
// fall back to memory and the database, and the health check reports Degraded
builder.AddRedisDistributedCache(
    "cache",
    configureOptions: options => options.BacklogPolicy = BacklogPolicy.FailFast);
builder.ReportHealthCheckFailureAsDegraded("StackExchange.Redis");

// Modules
builder.Services.AddApplicationModule(builder.Configuration);
builder.Services.AddInfrastructureModule(builder.Configuration);

// Database: health check, Npgsql traces and metrics, retries on transient errors
builder.EnrichNpgsqlDbContext<ApplicationDbContext>();

// Developer tools: pgweb, Storage Explorer and RedisInsight in the health report
builder.AddDevToolHealthCheck("pgweb");
builder.AddDevToolHealthCheck("storage-blobs-explorer");
builder.AddDevToolHealthCheck("redisinsight");

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
app.UseCors(corsPolicyName);

// Authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Minimal API
app.MapEndpoints();

// API docs
app.MapApiDocumentation();

// Aspire
app.MapDefaultEndpoints();

app.Run();