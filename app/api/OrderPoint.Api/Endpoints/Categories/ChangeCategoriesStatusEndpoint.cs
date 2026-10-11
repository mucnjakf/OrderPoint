using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Categories;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Categories;

internal sealed record ChangeCategoriesStatusRequest(IReadOnlyList<Guid> Ids, CategoryStatus Status);

internal sealed class ChangeCategoriesStatusEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/categories/status", HandleAsync)
            .WithName("ChangeCategoriesStatus")
            .WithTags("Categories")
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromBody] ChangeCategoriesStatusRequest request,
        [FromServices] IValidator<ChangeCategoriesStatusRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        ChangeCategoriesStatusCommand command = new(request.Ids, request.Status);

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }

    internal sealed class ChangeCategoriesStatusRequestValidator : AbstractValidator<ChangeCategoriesStatusRequest>
    {
        public ChangeCategoriesStatusRequestValidator()
        {
            RuleFor(request => request.Ids)
                .MustBeValidIds();

            RuleFor(request => request.Status)
                .IsInEnum().WithMessage("Status is invalid");
        }
    }
}