using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Commands.Suppliers;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Suppliers;

internal sealed class DeleteSupplierEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapDelete("api/suppliers/{id:guid}", HandleAsync)
            .WithName("DeleteSupplier")
            .WithTags("Suppliers")
            .WithSummary("Delete a supplier")
            .WithDescription("""
                Deletes the supplier; its stock items keep going without a supplier.

                - `409 Supplier.CannotDeleteWithDeliveries`: it delivered stock; deactivate it instead, so the history
                  stays.
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        Result result = await sender.Send(new DeleteSupplierCommand(id), cancellationToken);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.ToProblemDetails();
    }
}