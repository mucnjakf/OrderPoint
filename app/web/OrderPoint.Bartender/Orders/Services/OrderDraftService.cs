namespace OrderPoint.Bartender.Orders.Services;

// One per circuit: an order in progress for each table and for the counter, kept until it is sent
internal sealed class OrderDraftService
{
    private const string CounterKey = "";

    private readonly Dictionary<string, OrderDraft> _drafts = [];

    internal OrderDraft Get(string? tableCode)
    {
        string key = tableCode ?? CounterKey;

        if (!_drafts.TryGetValue(key, out OrderDraft? draft))
        {
            draft = new OrderDraft();
            _drafts[key] = draft;
        }

        return draft;
    }

    internal void Remove(string? tableCode)
    {
        _drafts.Remove(tableCode ?? CounterKey);
    }
}