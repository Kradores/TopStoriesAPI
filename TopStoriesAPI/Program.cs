using TopStoriesAPI.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSingleton<StoriesCacheService>();
builder.Services.AddHttpClient<StoriesRefreshWorker>();
builder.Services.AddHostedService(provider =>
    provider.GetRequiredService<StoriesRefreshWorker>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(o =>
    {
        o.SwaggerEndpoint("/openapi/v1.json", "Open API v1");
    });
}

app.UseAuthorization();

app.MapControllers();

app.Run();
