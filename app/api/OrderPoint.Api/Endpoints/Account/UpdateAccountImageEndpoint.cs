using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Account;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Account;

internal sealed record UpdateAccountImageRequest(IFormFile Image);

internal sealed class UpdateAccountImageEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/account/image", HandleAsync)
            .WithName("UpdateAccountImage")
            .WithTags("Account")
            .DisableAntiforgery()
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromForm] UpdateAccountImageRequest request,
        [FromServices] IValidator<UpdateAccountImageRequest> validator,
        [FromServices] ISender sender,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        await using Stream content = request.Image.OpenReadStream();

        UpdateAccountImageCommand command = new(user.GetUserId(), content, request.Image.ContentType);

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }

    internal sealed class UpdateAccountImageRequestValidator : AbstractValidator<UpdateAccountImageRequest>
    {
        public UpdateAccountImageRequestValidator()
        {
            RuleFor(request => request.Image).MustBeValidImage();
        }
    }
}