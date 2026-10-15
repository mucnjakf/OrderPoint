using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Suppliers;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Suppliers;

/// <summary>One supplier.</summary>
/// <param name="Data">The supplier.</param>
internal sealed record GetSupplierResponse(SupplierDto Data);

internal sealed class GetSupplierEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/suppliers/{id:guid}", HandleAsync)
            .WithName("GetSupplier")
            .WithTags("Suppliers")
            .WithSummary("Get a supplier")
            .WithDescription("""
                Returns one supplier with its counts and last delivery.
                """)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<Ok<GetSupplierResponse>, ProblemHttpResult>> HandleAsync(
        [FromRoute] Guid id,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        Result<SupplierDto> result = await sender.Send(new GetSupplierQuery(id), cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetSupplierResponse(result.Value))
            : result.ToProblemDetails();
    }
}