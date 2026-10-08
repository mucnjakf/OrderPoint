using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Auth;
using OrderPoint.Application.Dtos;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Auth;

internal sealed record RefreshTokensRequest(string RefreshToken);

internal sealed record RefreshTokensResponse(AuthTokensDto Data);

internal sealed class RefreshTokensEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPost("api/auth/refresh", HandleAsync)
            .WithName("RefreshTokens")
            .WithTags("Auth");
    }

    private static async Task<Results<Ok<RefreshTokensResponse>, ProblemHttpResult>> HandleAsync(
        [FromBody] RefreshTokensRequest request,
        [FromServices] IValidator<RefreshTokensRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        RefreshTokensCommand command = new(request.RefreshToken);

        Result<AuthTokensDto> result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new RefreshTokensResponse(result.Value))
            : result.ToProblemDetails();
    }

    internal sealed class RefreshTokensRequestValidator : AbstractValidator<RefreshTokensRequest>
    {
        public RefreshTokensRequestValidator()
        {
            RuleFor(request => request.RefreshToken)
                .NotEmpty().WithMessage("RefreshToken is required")
                .MaximumLength(200).WithMessage("RefreshToken must be at most 200 characters");
        }
    }
}