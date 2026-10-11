using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Bartenders;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Bartenders;

internal sealed record ChangeBartendersStatusRequest(IReadOnlyList<Guid> Ids, BartenderStatus Status);

internal sealed class ChangeBartendersStatusEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapPut("api/bartenders/status", HandleAsync)
            .WithName("ChangeBartendersStatus")
            .WithTags("Bartenders")
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromBody] ChangeBartendersStatusRequest request,
        [FromServices] IValidator<ChangeBartendersStatusRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        ChangeBartendersStatusCommand command = new(request.Ids, request.Status);

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }

    internal sealed class ChangeBartendersStatusRequestValidator : AbstractValidator<ChangeBartendersStatusRequest>
    {
        public ChangeBartendersStatusRequestValidator()
        {
            RuleFor(request => request.Ids)
                .MustBeValidIds();

            RuleFor(request => request.Status)
                .IsInEnum().WithMessage("Status is invalid");
        }
    }
}