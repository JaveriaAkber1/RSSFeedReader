using RSSFeedReader.Api.Models;

namespace RSSFeedReader.Api.Services;

public interface ISubscriptionService
{
    Subscription Add(string url);

    IReadOnlyList<Subscription> GetAll();
}
