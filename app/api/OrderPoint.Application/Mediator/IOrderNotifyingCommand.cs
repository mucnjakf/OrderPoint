namespace OrderPoint.Application.Mediator;

// A command that places an order or changes one; OrderNotificationBehavior tells the apps once it succeeds. Its
// handler adds the orders it changed to ChangedOrders, so the guests following them hear about it too.
internal interface IOrderNotifyingCommand;