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
        ("Account", "The signed-in user's own account: profile, profile image (admin only), password and sessions."),
        ("Categories", "Menu categories: create, read, update and delete, image, menu order and bulk actions."),
        ("Items", "Menu items: create, read, update and delete, image and bulk actions (status, category, price, " +
                  "delete)."),
        ("Bartenders", "Bartenders and their logins: create, read, update and delete, image, password reset and " +
                       "bulk actions."),
        ("Menu", "The menu as guests see it in the customer app: active categories with their active items. " +
                 "Anonymous."),
        ("Orders", "Orders placed by customers. Placing, tracking and cancelling an order are anonymous, admins and " +
                   "bartenders read them, and bartenders accept, decline, start and complete them."),
        ("Shift", "The signed-in bartender's figures for today, for the bartender app."),
        ("Areas", "Parts of the bar (Inside, Terrace), each with its floor plan grid, and the whole floor plan."),
        ("Tables", "The tables guests order from: code (in the QR code), seats, status and place on the floor plan. " +
                   "Looking a table up by its code is anonymous."),
        ("Fixtures", "Fixed pieces of a floor plan (bar counter, entrance, wall) that help staff find the tables."),
        ("Dashboard", "Aggregated numbers for the Admin dashboard, one endpoint per widget so they load in parallel."),
        ("Health", "The health of the API, the database and the blob storage, for the Admin's system health page.")
    ];

    private static readonly (string Name, string[] Tags)[] TagGroups =
    [
        ("Authentication", ["Auth", "Account"]),
        ("Menu", ["Categories", "Items", "Menu"]),
        ("Staff", ["Bartenders"]),
        ("Orders", ["Orders", "Shift"]),
        ("Floor plan", ["Areas", "Tables", "Fixtures"]),
        ("Insights", ["Dashboard"]),
        ("System", ["Health"])
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