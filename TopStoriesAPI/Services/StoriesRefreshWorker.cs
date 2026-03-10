using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;
using TopStoriesAPI.Models;

namespace TopStoriesAPI.Services;

public class StoriesRefreshWorker : BackgroundService
{
    private readonly HttpClient _httpClient;
    private readonly StoriesCacheService _cache;

    private const string BaseUrl = "https://hacker-news.firebaseio.com/v0";

    public StoriesRefreshWorker(HttpClient httpClient, StoriesCacheService cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await RefreshStories(stoppingToken);

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task RefreshStories(CancellationToken ct)
    {
        var ids = await _httpClient.GetFromJsonAsync<List<int>>($"{BaseUrl}/beststories.json", ct);

        if (ids == null)
            return;

        var stories = new ConcurrentBag<HackerNewsStory>();

        await Parallel.ForEachAsync(ids, new ParallelOptions
        {
            MaxDegreeOfParallelism = 20,
            CancellationToken = ct
        },
        async (id, token) =>
        {
            if (_cache.TryGetStory(id, out var cached))
            {
                stories.Add(cached);
                return;
            }

            var story = await _httpClient.GetFromJsonAsync<HackerNewsStory>(
                $"{BaseUrl}/item/{id}.json",
                token);

            if (story != null)
                stories.Add(story);
        });

        _cache.UpdateStories(stories);
    }
}