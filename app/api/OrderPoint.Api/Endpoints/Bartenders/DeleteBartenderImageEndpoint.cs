using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Bartenders;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Bartenders;

internal sealed class DeleteBartenderImageEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapDelete("api/bartenders/{id:guid}/image", HandleAsync)
            .WithName("DeleteBartenderImage")
            .WithTags("Bartenders");
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        DeleteBartenderImageCommand command = new(id);

        Result result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }
}