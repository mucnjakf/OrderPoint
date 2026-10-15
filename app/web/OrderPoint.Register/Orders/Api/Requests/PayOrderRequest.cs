using OrderPoint.Shared.Orders.Enumerations;

namespace OrderPoint.Register.Orders.Api.Requests;

// Cash with what the guest handed over, or a card the bar's own terminal charged
internal sealed record PayOrderRequest(PaymentMethod Method, decimal? CashReceived);