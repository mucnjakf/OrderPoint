using Microsoft.AspNetCore.Authentication.JwtBearer;
using Scalar.AspNetCore;

namespace OrderPoint.Api.OpenApi;

internal static class ApiDocumentationExtensions
{
    internal static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        return services.AddOpenApi(options => options
            .AddDocumentTransformer<DocumentInfoTransformer>()
            .AddSchemaTransformer<NumberSchemaTransformer>()
            .AddSchemaTransformer<EnumSchemaTransformer>()
            .AddSchemaTransformer<ProblemDetailsSchemaTransformer>()
            .AddSchemaTransformer<ValidationTransformer>()
            .AddOperationTransformer<ValidationTransformer>()
            .AddOperationTransformer<ResponsesTransformer>()
            .AddOperationTransformer<SecurityTransformer>());
    }

    internal static WebApplication MapApiDocumentation(this WebApplication app)
    {
        app.MapOpenApi();

        app.MapScalarApiReference(options =>
        {
            options
                .WithTitle("OrderPoint API")
                .WithClassicLayout()
                .WithTheme(ScalarTheme.Alternate)
                .ExpandAllTags()
                .SortOperationsByMethod()
                .AddPreferredSecuritySchemes(JwtBearerDefaults.AuthenticationScheme)
                .EnablePersistentAuthentication()
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);

            options.HideClientButton = true;
        });

        return app;
    }
}