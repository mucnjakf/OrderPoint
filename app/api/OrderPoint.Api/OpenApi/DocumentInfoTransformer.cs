using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace OrderPoint.Api.OpenApi;

internal sealed class DocumentInfoTransformer : IOpenApiDocumentTransformer
{
    private const string TagGroupsExtension = "x-tagGroups";

    private static readonly (string Name, string Description)[] Tags =
    [
        ("Auth", "Sign in, refresh and sign out. Anonymous; each app sends its own role, so an admin cannot sign in " +
                 "to the bartender app and the other way round."),
        ("Account", "The signed-in admin's own account: profile, profile image, password and sessions."),
        ("Categories", "Menu categories: create, read, update and delete, image, menu order and bulk actions."),
        ("Items", "Menu items: create, read, update and delete, image and bulk actions (status, category, price, " +
                  "delete)."),
        ("Bartenders", "Bartenders and their logins: create, read, update and delete, image, password reset and " +
                       "bulk actions."),
        ("Orders", "Orders placed by customers. Placing an order is anonymous; reading them requires the Admin role."),
        ("Dashboard", "Aggregated numbers for the Admin dashboard, one endpoint per widget so they load in parallel.")
    ];

    private static readonly (string Name, string[] Tags)[] TagGroups =
    [
        ("Authentication", ["Auth", "Account"]),
        ("Menu", ["Categories", "Items"]),
        ("Staff", ["Bartenders"]),
        ("Orders", ["Orders"]),
        ("Insights", ["Dashboard"])
    ];

    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Info = new OpenApiInfo
        {
            Title = "OrderPoint API",
            Version = "v1",
            Description = ApiGuide.Markdown
        };

        document.Tags = new HashSet<OpenApiTag>(Tags.Select(tag => new OpenApiTag
        {
            Name = tag.Name,
            Description = tag.Description
        }));

        JsonArray tagGroups = [];

        foreach ((string name, string[] tags) in TagGroups)
        {
            tagGroups.Add(new JsonObject
            {
                ["name"] = name,
                ["tags"] = new JsonArray(tags.Select(tag => (JsonNode?)JsonValue.Create(tag)).ToArray())
            });
        }

        document.Extensions ??= new Dictionary<string, IOpenApiExtension>();
        document.Extensions[TagGroupsExtension] = new JsonNodeExtension(tagGroups);

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[JwtBearerDefaults.AuthenticationScheme] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "The access token from `POST /api/auth/login` or `POST /api/auth/refresh`, " +
                          "valid for 15 minutes."
        };

        return Task.CompletedTask;
    }
}