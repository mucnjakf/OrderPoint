using MediatR;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Mediator;

internal interface ICommand : IRequest<Result>, IBaseCommand;

internal interface ICommand<TResponse> : IRequest<Result<TResponse>>, IBaseCommand;

internal interface IBaseCommand;