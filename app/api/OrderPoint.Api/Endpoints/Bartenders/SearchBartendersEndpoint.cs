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

internal sealed record SearchBartendersRequest([FromQuery] string SearchQuery);

internal sealed record SearchBartendersResponse(IReadOnlyList<BartenderDto> Data);

internal sealed class SearchBartendersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/bartenders/search", HandleAsync)
            .WithName("SearchBartenders")
            .WithTags("Bartenders")
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