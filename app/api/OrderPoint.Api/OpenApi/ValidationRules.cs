using System.Globalization;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Validators;
using Microsoft.OpenApi;

namespace OrderPoint.Api.OpenApi;

// Maps the FluentValidation rules of a request to JSON schema constraints; rules with a When condition and
// custom Must rules cannot be expressed in a schema and are described in text instead
internal static class ValidationRules
{
    private const string InclusiveBetweenValidatorName = "InclusiveBetweenValidator";

    internal static ILookup<string, IPropertyValidator>? Get(IServiceProvider services, Type requestType)
    {
        using IServiceScope scope = services.CreateScope();

        if (scope.ServiceProvider.GetService(typeof(IValidator<>).MakeGenericType(requestType))
            is not IValidator validator)
        {
            return null;
        }

        return validator
            .CreateDescriptor()
            .GetMembersWithValidators()
            .SelectMany(member => member
                .Where(rule => !rule.Options.HasCondition && !rule.Options.HasAsyncCondition)
                .Select(rule => (Member: member.Key, rule.Validator)))
            .ToLookup(rule => JsonNamingPolicy.CamelCase.ConvertName(rule.Member), rule => rule.Validator);
    }

    // Returns whether the rules make the value required
    internal static bool Apply(OpenApiSchema schema, IEnumerable<IPropertyValidator> validators)
    {
        bool isRequired = false;

        foreach (IPropertyValidator validator in validators)
        {
            switch (validator)
            {
                case INotNullValidator:
                    isRequired = true;
                    break;
                case INotEmptyValidator:
                    isRequired = true;
                    ApplyNotEmpty(schema);
                    break;
                case ILengthValidator length:
                    schema.MinLength = length.Min > 0 ? length.Min : schema.MinLength;
                    schema.MaxLength = length.Max > 0 ? length.Max : schema.MaxLength;
                    break;
                case IBetweenValidator between when validator.Name == InclusiveBetweenValidatorName:
                    schema.Minimum = Format(between.From);
                    schema.Maximum = Format(between.To);
                    break;
                case IBetweenValidator between:
                    schema.ExclusiveMinimum = Format(between.From);
                    schema.ExclusiveMaximum = Format(between.To);
                    break;
                case IComparisonValidator comparison:
                    ApplyComparison(schema, comparison);
                    break;
                case IEmailValidator:
                    schema.Format = "email";
                    break;
            }
        }

        return isRequired;
    }

    private static void ApplyNotEmpty(OpenApiSchema schema)
    {
        if (schema.Type?.HasFlag(JsonSchemaType.String) == true)
        {
            schema.MinLength = Math.Max(schema.MinLength ?? 0, 1);
        }

        if (schema.Type?.HasFlag(JsonSchemaType.Array) == true)
        {
            schema.MinItems = Math.Max(schema.MinItems ?? 0, 1);
        }
    }

    private static void ApplyComparison(OpenApiSchema schema, IComparisonValidator comparison)
    {
        string? value = Format(comparison.ValueToCompare);

        switch (comparison.Comparison)
        {
            case Comparison.GreaterThan:
                schema.ExclusiveMinimum = value;
                break;
            case Comparison.GreaterThanOrEqual:
                schema.Minimum = value;
                break;
            case Comparison.LessThan:
                schema.ExclusiveMaximum = value;
                break;
            case Comparison.LessThanOrEqual:
                schema.Maximum = value;
                break;
        }
    }

    private static string? Format(object? value)
    {
        return value is null ? null : Convert.ToString(value, CultureInfo.InvariantCulture);
    }
}