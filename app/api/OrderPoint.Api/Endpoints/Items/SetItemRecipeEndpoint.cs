using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Endpoints.Stock;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Items;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Items;

/// <summary>One ingredient.</summary>
/// <param name="StockItemId">The stock item it uses.</param>
/// <param name="Quantity">How much one serving uses, in the stock item's base unit, e.g. 50 (ml).</param>
internal sealed record SetItemRecipeLineRequest(Guid StockItemId, decimal Quantity);

/// <summary>The item's whole recipe.</summary>
/// <param name="Lines">The ingredients, each stock item at most once; empty stops tracking the item's stock.</param>
internal sealed record SetItemRecipeRequest(IReadOnlyList<SetItemRecipeLineRequest> Lines);

internal sealed class SetItemRecipeEndpoint : IEndpoint
{
    private const int MaxLines = 30;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/items/{id:guid}/recipe", HandleAsync)
            .WithName("SetItemRecipe")
            .WithTags("Items")
            .WithSummary("Set an item's recipe")
            .WithDescription($"""
                Replaces the item's recipe: what one serving uses of each stock item. Completing an order takes it
                off the stock, and the item is sold out (not on the menu, refused when ordered) while an ingredient
                has less than one serving left. At most {MaxLines} ingredients, each stock item at most once.

                - `404 Item.NotFound`: the item does not exist
                - `404 StockItem.NotFound`: a stock item does not exist
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromBody] SetItemRecipeRequest request,
        [FromServices] IValidator<SetItemRecipeRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        SetItemRecipeCommand command = new(
            id,
            request.Lines.Select(line => new SetItemRecipeLineCommand(line.StockItemId, line.Quantity)).ToList());

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }

    internal sealed class SetItemRecipeRequestValidator : AbstractValidator<SetItemRecipeRequest>
    {
        public SetItemRecipeRequestValidator()
        {
            RuleFor(request => request.Lines)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Lines are required")
                .Must(lines => lines.Count <= MaxLines).WithMessage($"Lines must be at most {MaxLines}")
                .Must(lines => lines.DistinctBy(line => line.StockItemId).Count() == lines.Count)
                .WithMessage("Each stock item may be in the recipe only once");

            RuleForEach(request => request.Lines).ChildRules(line =>
            {
                line.RuleFor(recipeLine => recipeLine.StockItemId)
                    .NotEmpty().WithMessage("StockItemId is required");

                line.RuleFor(recipeLine => recipeLine.Quantity)
                    .GreaterThan(0).WithMessage("Quantity must be positive")
                    .LessThanOrEqualTo(CreateStockItemEndpoint.MaxQuantity)
                    .WithMessage($"Quantity must be at most {CreateStockItemEndpoint.MaxQuantity}");
            });
        }
    }
}