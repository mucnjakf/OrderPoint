using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Categories;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Categories;

internal sealed record ReorderCategoriesRequest(IReadOnlyList<Guid> Ids);

internal sealed class ReorderCategoriesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/categories/display-order", HandleAsync)
            .WithName("ReorderCategories")
            .WithTags("Categories")
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromBody] ReorderCategoriesRequest request,
        [FromServices] IValidator<ReorderCategoriesRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        ReorderCategoriesCommand command = new(request.Ids);

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }

    internal sealed class ReorderCategoriesRequestValidator : AbstractValidator<ReorderCategoriesRequest>
    {
        public ReorderCategoriesRequestValidator()
        {
            RuleFor(request => request.Ids)
                .MustBeValidIds();
        }
    }
}