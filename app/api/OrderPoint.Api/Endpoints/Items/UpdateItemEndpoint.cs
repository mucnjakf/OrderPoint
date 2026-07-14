using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Items;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Items;

public sealed record UpdateItemRequest(
    string Name,
    string Description,
    double Portion,
    decimal Price,
    string? ImageUrl,
    Guid CategoryId);

internal sealed class UpdateItemEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/items/{id:guid}", HandleAsync)
            .WithName("UpdateItem")
            .WithTags("Items");
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateItemRequest request,
        [FromServices] IValidator<UpdateItemRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        UpdateItemCommand command = new(
            id,
            request.Name,
            request.Description,
            request.Portion,
            request.Price,
            request.ImageUrl,
            request.CategoryId);

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }

    internal sealed class UpdateItemRequestValidator : AbstractValidator<UpdateItemRequest>
    {
        public UpdateItemRequestValidator()
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