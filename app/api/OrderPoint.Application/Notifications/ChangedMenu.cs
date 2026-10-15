namespace OrderPoint.Application.Notifications;

// One per request: set by a handler whose stock change made a menu item sold out or available again (known only once
// it ran), so MenuNotificationBehavior tells the apps although the command isn't a menu command
internal sealed class ChangedMenu
{
    internal bool IsChanged { get; private set; }

    internal void MarkChanged()
    {
        IsChanged = true;
    }
}