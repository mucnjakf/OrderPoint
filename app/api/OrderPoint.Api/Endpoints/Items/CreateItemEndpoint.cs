using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Items;
using OrderPoint.Application.Dtos;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Items;

public sealed record CreateItemRequest(
    string Name,
    string Description,
    double Portion,
    decimal Price,
    string? ImageUrl,
    Guid CategoryId);

internal sealed record CreateItemResponse(ItemDto Data);

internal sealed class CreateItemEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPost("api/items", HandleAsync)
            .WithName("CreateItem")
            .WithTags("Items");
    }

    private static async Task<Results<CreatedAtRoute<CreateItemResponse>, ProblemHttpResult>> HandleAsync(
        [FromBody] CreateItemRequest request,
        [FromServices] IValidator<CreateItemRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        CreateItemCommand command = new(
            request.Name,
            request.Description,
            request.Portion,
            request.Price,
            request.ImageUrl,
            request.CategoryId);

        Result<ItemDto> result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.CreatedAtRoute(
                new CreateItemResponse(result.Value),
                "GetItem",
                new { id = result.Value.Id })
            : result.ToProblemDetails();
    }

    internal sealed class CreateItemRequestValidator : AbstractValidator<CreateItemRequest>
    {
        public CreateItemRequestValidator()
        {
            RuleFor(request => request.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(30).WithMessage("Name must be at most 30 characters");

            RuleFor(request => request.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(200).WithMessage("Description must be at most 200 characters");

            RuleFor(request => request.Portion)
                .GreaterThan(0).WithMessage("Portion must be positive");

            RuleFor(request => request.Price)
                .GreaterThan(0).WithMessage("Price must be positive");

            RuleFor(request => request.ImageUrl)
                .MaximumLength(200).WithMessage("ImageUrl must be at most 200 characters");
        }
    }
}