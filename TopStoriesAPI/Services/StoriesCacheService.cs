using System.Collections.Concurrent;
using TopStoriesAPI.Models;

namespace TopStoriesAPI.Services;

public class StoriesCacheService
{
    private readonly ConcurrentDictionary<int, HackerNewsStory> _stories = new();
    public bool IsReady => !_stories.IsEmpty;

    public IEnumerable<HackerNewsStory> GetStories(int limit)
    {
        return _stories.Values
            .OrderByDescending(s => s.Score)
            .Take(limit);
    }

    public void UpdateStories(IEnumerable<HackerNewsStory> stories)
    {
        foreach (var story in stories)
        {
            _stories[story.Id] = story;
        }
    }

    public bool TryGetStory(int id, out HackerNewsStory story)
    {
        return _stories.TryGetValue(id, out story);
    }
}