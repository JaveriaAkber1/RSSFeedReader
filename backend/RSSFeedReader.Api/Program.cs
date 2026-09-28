using RSSFeedReader.Api.Models;
using RSSFeedReader.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<ISubscriptionService, InMemorySubscriptionService>();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendOnly", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("FrontendOnly");

app.MapPost("/api/subscriptions", (SubscriptionRequest request, ISubscriptionService subscriptionService) =>
{
    if (string.IsNullOrWhiteSpace(request.Url))
    {
        return Results.BadRequest(new { error = "url must not be blank" });
    }

    var subscription = subscriptionService.Add(request.Url);
    return Results.Created("/api/subscriptions", subscription);
})
.WithName("AddSubscription");

app.MapGet("/api/subscriptions", (ISubscriptionService subscriptionService) =>
{
    return Results.Ok(subscriptionService.GetAll());
})
.WithName("GetSubscriptions");

app.Run();

record SubscriptionRequest(string Url);
