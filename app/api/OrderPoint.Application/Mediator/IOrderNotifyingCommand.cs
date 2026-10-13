namespace OrderPoint.Application.Mediator;

// A command that places an order or changes its status; OrderNotificationBehavior tells the apps once it succeeds
internal interface IOrderNotifyingCommand
{
    // The order whose guest may be following it; null when nobody can be yet (an order being placed)
    Guid? TrackedOrderId { get; }
}