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

internal sealed record AdjustItemsPriceRequest(IReadOnlyList<Guid> Ids, PriceAdjustmentMode Mode, decimal Value);

internal sealed class AdjustItemsPriceEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/items/price", HandleAsync)
            .WithName("AdjustItemsPrice")
            .WithTags("Items")
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