using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Categories;
using OrderPoint.Application.Dtos;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Categories;

/// <summary>The new category.</summary>
/// <param name="Name">The name shown on the menu.</param>
/// <param name="Description">A short description shown on the menu.</param>
/// <param name="Status">The status. Items in an inactive category cannot be ordered.</param>
internal sealed record CreateCategoryRequest(
    string Name,
    string Description,
    CategoryStatus Status);

/// <summary>The created category.</summary>
/// <param name="Data">The category.</param>
internal sealed record CreateCategoryResponse(CategoryDto Data);

internal sealed class CreateCategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPost("api/categories", HandleAsync)
            .WithName("CreateCategory")
            .WithTags("Categories")
            .WithSummary("Create a category")
            .WithDescription("""
                Creates a category at the end of the menu (`displayOrder` is the highest so far plus one).
                """)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<CreatedAtRoute<CreateCategoryResponse>, ProblemHttpResult>> HandleAsync(
        [FromBody] CreateCategoryRequest request,
        [FromServices] IValidator<CreateCategoryRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        CreateCategoryCommand command = new(
            request.Name,
            request.Description,
            request.Status);

        Result<CategoryDto> result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.CreatedAtRoute(
                new CreateCategoryResponse(result.Value),
                "GetCategory",
                new { id = result.Value.Id })
            : result.ToProblemDetails();
    }

    internal sealed class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
    {
        public CreateCategoryRequestValidator()
        {
            RuleFor(request => request.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(30).WithMessage("Name must be at most 30 characters");

            RuleFor(request => request.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(100).WithMessage("Description must be at most 100 characters");

            RuleFor(request => request.Status)
                .IsInEnum().WithMessage("Status is invalid");
        }
    }
}