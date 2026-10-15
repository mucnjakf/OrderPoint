using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Suppliers;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;
using OrderPoint.Domain.Sorting;

namespace OrderPoint.Api.Endpoints.Suppliers;

/// <summary>Paging, search, filter and sort options.</summary>
/// <param name="PageNumber">The page to return, starting at 1.</param>
/// <param name="PageSize">The number of suppliers per page.</param>
/// <param name="SearchQuery">Matches the name or the contact, ignoring case.</param>
/// <param name="Status">Only suppliers with this status.</param>
/// <param name="SortBy">The sort order. Default: `NameAsc`.</param>
internal sealed record GetSuppliersRequest(
    [FromQuery] int PageNumber,
    [FromQuery] int PageSize,
    [FromQuery] string? SearchQuery,
    [FromQuery] SupplierStatus? Status,
    [FromQuery] SupplierSortBy? SortBy);

/// <summary>A page of suppliers.</summary>
/// <param name="Data">The page.</param>
internal sealed record GetSuppliersResponse(PaginationDto<SupplierDto> Data);

internal sealed class GetSuppliersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/suppliers", HandleAsync)
            .WithName("GetSuppliers")
            .WithTags("Suppliers")
            .WithSummary("List suppliers")
            .WithDescription("""
                A page of suppliers with search (name or contact), a status filter and sorting (default: by name),
                each with how many stock items are bought from it, how many need reordering and its last delivery.
                """)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<Ok<GetSuppliersResponse>, ProblemHttpResult>> HandleAsync(
        [AsParameters] GetSuppliersRequest request,
        [FromServices] IValidator<GetSuppliersRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        GetSuppliersQuery query = new(
            request.PageNumber,
            request.PageSize,
            request.SearchQuery,
            request.Status,
            request.SortBy);

        Result<PaginationDto<SupplierDto>> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetSuppliersResponse(result.Value))
            : result.ToProblemDetails();
    }

    internal sealed class GetSuppliersRequestValidator : AbstractValidator<GetSuppliersRequest>
    {
        public GetSuppliersRequestValidator()
        {
            RuleFor(request => request.PageNumber)
                .GreaterThan(0).WithMessage("PageNumber must be positive");

            RuleFor(request => request.PageSize)
                .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100");

            RuleFor(request => request.SearchQuery)
                .MaximumLength(100).WithMessage("SearchQuery must be at most 100 characters");

            RuleFor(request => request.Status)
                .IsInEnum().WithMessage("Status is invalid");

            RuleFor(request => request.SortBy)
                .IsInEnum().WithMessage("SortBy is invalid");
        }
    }
}