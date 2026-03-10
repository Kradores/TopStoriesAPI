using Microsoft.AspNetCore.Mvc;
using TopStoriesAPI.Models;
using TopStoriesAPI.Services;

namespace TopStoriesAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StoriesController : ControllerBase
    {
        private readonly StoriesCacheService _cache;

        public StoriesController(StoriesCacheService cache)
        {
            _cache = cache;
        }

        [HttpGet]
        public IActionResult Get(int limit = 50)
        {
            if (!_cache.IsReady)
            {
                return StatusCode(
                    StatusCodes.Status503ServiceUnavailable,
                    new { message = "Stories are loading, please retry shortly." });
            }

            return Ok(_cache.GetStories(limit).Select(MapToResponse));
        }

        private static StoryResponse MapToResponse(HackerNewsStory story)
        {
            return new StoryResponse
            {
                Title = story.Title,
                Uri = story.Url,
                PostedBy = story.By,
                Time = DateTimeOffset
                    .FromUnixTimeSeconds(story.Time)
                    .ToString("yyyy-MM-ddTHH:mm:sszzz"),
                Score = story.Score,
                CommentCount = story.Descendants
            };
        }
    }
}
