using System.Reflection;
using FluentValidation;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Exceptions;
using OrderPoint.Api.Extensions;
using OrderPoint.Application;
using OrderPoint.Infrastructure;
using OrderPoint.ServiceDefaults;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Aspire
builder.AddServiceDefaults();

// API docs
builder.Services.AddOpenApi();

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
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Minimal API
builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>(includeInternalTypes: true);

// Modules
builder.Services.AddApplicationModule(builder.Configuration);
builder.Services.AddInfrastructureModule(builder.Configuration);

// ---------------------------------------------------------------------------------------------------------------------

WebApplication app = builder.Build();

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

// Cors
app.UseCors("AllowAll");

// Database
app.ApplyMigrations();

// HTTP
app.UseHttpsRedirection();

// Error handling
app.UseExceptionHandler();

// Aspire
app.MapDefaultEndpoints();

app.Run();