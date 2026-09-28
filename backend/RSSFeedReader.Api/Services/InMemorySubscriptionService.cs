using RSSFeedReader.Api.Models;

namespace RSSFeedReader.Api.Services;

public class InMemorySubscriptionService : ISubscriptionService
{
    private readonly List<Subscription> _subscriptions = [];
    private readonly Lock _lock = new();

    public Subscription Add(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("url must not be blank", nameof(url));
        }

        var subscription = new Subscription(url);

        lock (_lock)
        {
            _subscriptions.Add(subscription);
        }

        return subscription;
    }

    public IReadOnlyList<Subscription> GetAll()
    {
        lock (_lock)
        {
            return _subscriptions.ToList();
        }
    }
}
