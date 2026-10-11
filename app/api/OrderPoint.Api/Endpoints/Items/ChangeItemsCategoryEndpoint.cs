using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Items;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Items;

internal sealed record ChangeItemsCategoryRequest(IReadOnlyList<Guid> Ids, Guid CategoryId);

internal sealed class ChangeItemsCategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/items/category", HandleAsync)
            .WithName("ChangeItemsCategory")
            .WithTags("Items")
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromBody] ChangeItemsCategoryRequest request,
        [FromServices] IValidator<ChangeItemsCategoryRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        ChangeItemsCategoryCommand command = new(request.Ids, request.CategoryId);

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }

    internal sealed class ChangeItemsCategoryRequestValidator : AbstractValidator<ChangeItemsCategoryRequest>
    {
        public ChangeItemsCategoryRequestValidator()
        {
            RuleFor(request => request.Ids)
                .MustBeValidIds();

            RuleFor(request => request.CategoryId)
                .NotEmpty().WithMessage("CategoryId is required");
        }
    }
}