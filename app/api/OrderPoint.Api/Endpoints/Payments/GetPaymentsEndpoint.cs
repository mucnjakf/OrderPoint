using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OrderPoint.Api.Configuration;
using OrderPoint.Api.Extensions;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Queries.Payments;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;
using OrderPoint.Domain.Sorting;

namespace OrderPoint.Api.Endpoints.Payments;

/// <summary>Paging, search, period, filter and sort options.</summary>
/// <param name="PageNumber">The page to return, starting at 1.</param>
/// <param name="PageSize">The number of payments per page.</param>
/// <param name="SearchQuery">Matches the order number, the table code or a counter order's customer name.</param>
/// <param name="Period">Only payments in this period: `Today` (since the business day started, e.g. 06:00 local
/// time), `Last7Days` or `Last30Days` (both including today). Without it, every payment.</param>
/// <param name="TimeZone">The IANA time zone of the viewer, e.g. `Europe/Zagreb`; required with a period.</param>
/// <param name="Channel">Only payments that came in this way: `Cash` or `CardAtRegister` (the bar's terminal) at the
/// register, or `CardOnline` (a guest's card in the customer app).</param>
/// <param name="PaidByBartenderId">Only payments this bartender took at the register.</param>
/// <param name="PaymentStatus">Only payments in this state: `Paid`, `RefundDue` or `Refunded`.</param>
/// <param name="SortBy">The sort order. Default: `PaidAtUtcDesc` (the latest payment first).</param>
internal sealed record GetPaymentsRequest(
    [FromQuery] int PageNumber,
    [FromQuery] int PageSize,
    [FromQuery] string? SearchQuery,
    [FromQuery] DashboardPeriod? Period,
    [FromQuery] string? TimeZone,
    [FromQuery] PaymentChannel? Channel,
    [FromQuery] Guid? PaidByBartenderId,
    [FromQuery] PaymentStatus? PaymentStatus,
    [FromQuery] PaymentSortBy? SortBy);

/// <summary>A page of payments.</summary>
/// <param name="Data">The page: the paid orders, each with its payment.</param>
internal sealed record GetPaymentsResponse(PaginationDto<OrderDto> Data);

internal sealed class GetPaymentsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app
            .MapGet("api/payments", HandleAsync)
            .WithName("GetPayments")
            .WithTags("Payments")
            .WithSummary("List payments")
            .WithDescription("""
                A page of paid orders (paid, refund due or refunded; an order is paid once, so each is one payment)
                with search (order number, table code or customer name), a period by payment time, filters (how it
                came in, the bartender who took it, its state) and sorting (default: the latest payment first).
                `total` is the amount paid.

                - `timeZone` is required with `period`, and ignored without it.
                - `paymentStatus` can't be `Unpaid`: an unpaid order has no payment yet (see
                  `GET api/orders?payment=ToPay`).
                """)
            .RequireAuthorization(AuthorizationPolicies.AdminOrBartender);
    }

    private static async Task<Results<Ok<GetPaymentsResponse>, ProblemHttpResult>> HandleAsync(
        [AsParameters] GetPaymentsRequest request,
        [FromServices] IValidator<GetPaymentsRequest> validator,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        GetPaymentsQuery query = new(
            request.PageNumber,
            request.PageSize,
            request.SearchQuery,
            request.Period,
            request.Period is null ? null : TimeZoneInfo.FindSystemTimeZoneById(request.TimeZone!),
            request.Channel,
            request.PaidByBartenderId,
            request.PaymentStatus,
            request.SortBy);

        Result<PaginationDto<OrderDto>> result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(new GetPaymentsResponse(result.Value))
            : result.ToProblemDetails();
    }

    internal sealed class GetPaymentsRequestValidator : AbstractValidator<GetPaymentsRequest>
    {
        public GetPaymentsRequestValidator()
        {
            RuleFor(request => request.PageNumber)
                .GreaterThan(0).WithMessage("PageNumber must be positive");

            RuleFor(request => request.PageSize)
                .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100");

            RuleFor(request => request.SearchQuery)
                .MaximumLength(100).WithMessage("SearchQuery must be at most 100 characters");

            RuleFor(request => request.Period)
                .IsInEnum().WithMessage("Period is invalid");

            RuleFor(request => request.TimeZone!)
                .MustBeValidTimeZone()
                .When(request => request.Period is not null);

            RuleFor(request => request.Channel)
                .IsInEnum().WithMessage("Channel is invalid");

            RuleFor(request => request.PaymentStatus)
                .IsInEnum().WithMessage("PaymentStatus is invalid")
                .NotEqual(PaymentStatus.Unpaid).WithMessage("PaymentStatus can't be Unpaid");

            RuleFor(request => request.SortBy)
                .IsInEnum().WithMessage("SortBy is invalid");
        }
    }
}