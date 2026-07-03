using MediatR;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Mediator;

internal interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;