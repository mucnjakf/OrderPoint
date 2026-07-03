using MediatR;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Mediator;

internal interface IQuery<TResponse> : IRequest<Result<TResponse>>;