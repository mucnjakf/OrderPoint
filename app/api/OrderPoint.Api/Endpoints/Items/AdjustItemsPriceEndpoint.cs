using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Items;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Items;

/// <summary>The items and how to change their prices.</summary>
/// <param name="Ids">The ids of 1 to 100 different items.</param>
/// <param name="Mode">How `value` is applied to every price.</param>
/// <param name="Value">The new price in euros for `Set` (must be positive), the percentage for `ChangeByPercent`
/// (above -100, not 0) or the amount in euros for `ChangeByAmount` (not 0, negative lowers the price).</param>
internal sealed record AdjustItemsPriceRequest(IReadOnlyList<Guid> Ids, PriceAdjustmentMode Mode, decimal Value);

internal sealed class AdjustItemsPriceEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/items/price", HandleAsync)
            .WithName("AdjustItemsPrice")
            .WithTags("Items")
            .WithSummary("Adjust the price of items")
            .WithDescription("""
                Changes the price of up to 100 items at once:

                - `Set` gives every item the price `value`.
                - `ChangeByPercent` multiplies every price by (1 + `value` / 100); `value` must be above -100.
                - `ChangeByAmount` adds `value` (in euros) to every price; a negative value lowers it.

                New prices are rounded to 2 decimals (half away from zero). Existing order lines keep their price.
                All-or-nothing: if any id does not exist (`404`), nothing changes.

                - `400 Item.PriceMustBePositive`: a new price would be 0 or less; nothing changes.
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromBody] AdjustItemsPriceRequest request,
        [FromServices] IValidator<AdjustItemsPriceRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        AdjustItemsPriceCommand command = new(request.Ids, request.Mode, request.Value);

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }

    internal sealed class AdjustItemsPriceRequestValidator : AbstractValidator<AdjustItemsPriceRequest>
    {
        private const decimal MinPercent = -100;

        public AdjustItemsPriceRequestValidator()
        {
            RuleFor(request => request.Ids)
                .MustBeValidIds();

            RuleFor(request => request.Mode)
                .IsInEnum().WithMessage("Mode is invalid");

            RuleFor(request => request.Value)
                .GreaterThan(0).WithMessage("Value must be positive")
                .When(request => request.Mode is PriceAdjustmentMode.Set);

            RuleFor(request => request.Value)
                .NotEqual(0).WithMessage("Value must not be zero")
                .When(request => request.Mode is not PriceAdjustmentMode.Set);

            RuleFor(request => request.Value)
                .GreaterThan(MinPercent).WithMessage("Value must be greater than -100")
                .When(request => request.Mode is PriceAdjustmentMode.ChangeByPercent);
        }
    }
}