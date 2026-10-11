using System.Reflection;
using FluentValidation.Validators;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace OrderPoint.Api.OpenApi;

// Copies the request validators' limits into the request body schemas and the query parameters
internal sealed class ValidationTransformer : IOpenApiSchemaTransformer, IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (context.JsonPropertyInfo is not null || schema.Properties is null)
        {
            return Task.CompletedTask;
        }

        ILookup<string, IPropertyValidator>? rules =
            ValidationRules.Get(context.ApplicationServices, context.JsonTypeInfo.Type);

        if (rules is null)
        {
            return Task.CompletedTask;
        }

        foreach ((string name, IOpenApiSchema property) in schema.Properties)
        {
            if (property is not OpenApiSchema concreteProperty || !rules.Contains(name))
            {
                continue;
            }

            if (ValidationRules.Apply(concreteProperty, rules[name]))
            {
                schema.Required ??= new HashSet<string>();
                schema.Required.Add(name);
            }
        }

        return Task.CompletedTask;
    }

    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        Type? queryType = context.Description.ActionDescriptor.EndpointMetadata
            .OfType<MethodInfo>()
            .SelectMany(method => method.GetParameters())
            .FirstOrDefault(parameter => parameter.GetCustomAttribute<AsParametersAttribute>() is not null)
            ?.ParameterType;

        if (queryType is null || operation.Parameters is null)
        {
            return Task.CompletedTask;
        }

        ILookup<string, IPropertyValidator>? rules = ValidationRules.Get(context.ApplicationServices, queryType);

        if (rules is null)
        {
            return Task.CompletedTask;
        }

        foreach (IOpenApiParameter parameter in operation.Parameters)
        {
            string name = System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(parameter.Name ?? string.Empty);

            if (parameter is not OpenApiParameter concreteParameter ||
                concreteParameter.Schema is not OpenApiSchema schema ||
                !rules.Contains(name))
            {
                continue;
            }

            if (ValidationRules.Apply(schema, rules[name]))
            {
                concreteParameter.Required = true;
            }
        }

        return Task.CompletedTask;
    }
}