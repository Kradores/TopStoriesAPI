# Top Stories API

A small ASP.NET Core Web API that retrieves the best stories from Hacker News and exposes them through a simple endpoint.

The API is optimized for high performance and low latency by caching stories in memory and refreshing them periodically in the background.

---

# Requirements

To run the project you need:

* .NET 8 SDK (or newer)

You can verify installation with:

```
dotnet --version
```

---

# Running the Project

Clone the repository and navigate to the project directory:

```
git clone <repository-url>
cd TopStoriesAPI\TopStoriesAPI
```

Restore dependencies:

```
dotnet restore
```

Run the application:

```
dotnet run
```

The API will start and display the listening URL in the console, typically:

```
http://localhost:5215
```

---

# API Endpoint

## Get Top Stories

```
GET /api/stories?limit=50
```

### Query Parameters

| Parameter | Description                               |
| --------- | ----------------------------------------- |
| limit     | Number of stories to return (default: 50) |

### Example Request

```
curl "http://localhost:5215/api/stories?limit=10"
```

### Example Response

```
[
  {
    "title": "A uBlock Origin update was rejected from the Chrome Web Store",
    "uri": "https://github.com/uBlockOrigin/uBlock-issues/issues/745",
    "postedBy": "ismaildonmez",
    "time": "2019-10-12T13:43:01+00:00",
    "score": 1716,
    "commentCount": 572
  }
]
```

---

# Cold Start Behavior

When the service starts for the first time, it needs to fetch stories from the Hacker News API.

This takes approximately **10–12 seconds**.

During this period the API will return:

```
503 Service Unavailable
```

Example response:

```
{
  "message": "Stories are loading, please retry shortly."
}
```

Once the cache is populated, all subsequent requests return immediately.

---

# Architecture

The application uses three main components:

### Controller

Handles HTTP requests and returns cached stories.

```
Client -> StoriesController -> StoriesCacheService
```

---

### Cache Service

Stores stories in memory using a thread-safe structure.

```
ConcurrentDictionary<int, Story>
```

Benefits:

* O(1) story lookup by ID
* extremely fast reads
* safe under high concurrency

---

### Background Worker

A hosted background service refreshes the stories every minute.

Workflow:

```
Background Worker -> Fetch best story IDs -> Fetch stories concurrently -> Update in-memory cache
```

---

# Why This Design

Directly fetching stories from the Hacker News API for every request would require up to **200 external HTTP calls**, resulting in very slow response times (~12 seconds).
Amd if we get just 10 requests a second, that jumps to 2000 requests/second to Hacker News API.

Instead, the application uses **in-memory caching with periodic refresh**.

Benefits:

* Requests return in **~1ms**
* External API load remains constant
* Scales to thousands of requests per second
* Logic remains simple and predictable

---

# Data Refresh Strategy

Every minute the background worker:

1. Fetches the list of best story IDs
2. Retrieves the corresponding stories concurrently
3. Updates the in-memory cache

Because the Hacker News API has no strict rate limits, refreshing **200 stories per minute** is acceptable and keeps the implementation simple.

---

# Concurrency

Stories are fetched concurrently using:

```
Parallel.ForEachAsync
```

with a limited level of parallelism to avoid excessive network usage.

This significantly reduces refresh time compared to sequential requests.

---

# Thread Safety

The in-memory cache uses:

```
ConcurrentDictionary
```

This ensures safe concurrent access when:

* the background worker updates stories
* the API serves multiple requests simultaneously

---

# Summary

This API was designed to be:

* fast
* simple
* scalable
* resilient to external API latency

By separating **data fetching** from **request handling**, the system can serve responses instantly while still keeping data reasonably fresh.
