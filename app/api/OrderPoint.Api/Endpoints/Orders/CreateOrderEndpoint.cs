using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Orders;
using OrderPoint.Application.Dtos;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Orders;

internal sealed record CreateOrderItemRequest(Guid ItemId, int Quantity);

internal sealed record CreateOrderRequest(
    string TableCode,
    string? Note,
    IReadOnlyList<CreateOrderItemRequest> Items);

internal sealed record CreateOrderResponse(OrderDto Data);

internal sealed class CreateOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPost("api/orders", HandleAsync)
            .WithName("CreateOrder")
            .WithTags("Orders");
    }

    private static async Task<Results<CreatedAtRoute<CreateOrderResponse>, ProblemHttpResult>> HandleAsync(
        [FromBody] CreateOrderRequest request,
        [FromServices] IValidator<CreateOrderRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        CreateOrderCommand command = new(
            request.TableCode,
            request.Note,
            request.Items.Select(item => new CreateOrderItemCommand(item.ItemId, item.Quantity)).ToList());

        Result<OrderDto> result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.CreatedAtRoute(
                new CreateOrderResponse(result.Value),
                "GetOrder",
                new { id = result.Value.Id })
            : result.ToProblemDetails();
    }

    internal sealed class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
    {
        public CreateOrderRequestValidator()
        {
            RuleFor(request => request.TableCode)
                .NotEmpty().WithMessage("TableCode is required")
                .MaximumLength(10).WithMessage("TableCode must be at most 10 characters");

            RuleFor(request => request.Note)
                .MaximumLength(200).WithMessage("Note must be at most 200 characters");

            RuleFor(request => request.Items)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Items are required")
                .Must(items => items.DistinctBy(item => item.ItemId).Count() == items.Count)
                .WithMessage("Items must be unique");

            RuleForEach(request => request.Items).ChildRules(item =>
            {
                item.RuleFor(orderItem => orderItem.ItemId)
                    .NotEmpty().WithMessage("ItemId is required");

                item.RuleFor(orderItem => orderItem.Quantity)
                    .GreaterThan(0).WithMessage("Quantity must be positive");
            });
        }
    }
}