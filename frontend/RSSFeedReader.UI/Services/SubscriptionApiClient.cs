using System.Net.Http.Json;

namespace RSSFeedReader.UI.Services;

public class SubscriptionApiClient(HttpClient httpClient)
{
    public async Task<SubscriptionDto?> AddSubscriptionAsync(string url)
    {
        var response = await httpClient.PostAsJsonAsync("api/subscriptions", new SubscriptionDto(url));

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<SubscriptionDto>();
    }

    public async Task<List<SubscriptionDto>> GetSubscriptionsAsync()
    {
        var subscriptions = await httpClient.GetFromJsonAsync<List<SubscriptionDto>>("api/subscriptions");
        return subscriptions ?? [];
    }
}

public record SubscriptionDto(string Url);
