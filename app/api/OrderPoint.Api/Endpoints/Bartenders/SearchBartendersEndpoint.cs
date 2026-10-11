using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Bartenders;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Endpoints.Bartenders;

/// <summary>The search text.</summary>
/// <param name="SearchQuery">Matches the first or the last name, ignoring case.</param>
internal sealed record SearchBartendersRequest([FromQuery] string SearchQuery);

/// <summary>The matching bartenders.</summary>
/// <param name="Data">The bartenders, sorted by name.</param>
internal sealed record SearchBartendersResponse(IReadOnlyList<BartenderDto> Data);

internal sealed class SearchBartendersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/bartenders/search", HandleAsync)
            .WithName("SearchBartenders")
            .WithTags("Bartenders")
            .WithSummary("Search bartenders")
            .WithDescription("""
                An unpaged lookup by first or last name for autocompletes, sorted by name (last name, then first
                name).
                """)
            .RequireAuthorization(AuthorizationPolicies.Admin);
    }

    private static async Task<Results<Ok<SearchBartendersResponse>, ProblemHttpResult>> HandleAsync(
        [AsParameters] SearchBartendersRequest request,
        [FromServices] IValidator<SearchBartendersRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        SearchBartendersQuery query = new(request.SearchQuery);

        Result<IReadOnlyList<BartenderDto>> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new SearchBartendersResponse(result.Value))
            : result.ToProblemDetails();
    }

    internal sealed class SearchBartendersRequestValidator : AbstractValidator<SearchBartendersRequest>
    {
        public SearchBartendersRequestValidator()
        {
            RuleFor(request => request.SearchQuery)
                .NotEmpty().WithMessage("SearchQuery is required")
                .MaximumLength(100).WithMessage("SearchQuery must be at most 100 characters");
        }
    }
}