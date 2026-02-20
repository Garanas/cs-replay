var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("FAForeverReplay", client =>
{
    client.BaseAddress = new Uri("https://replay.faforever.com/");
    client.DefaultRequestHeaders.Add("User-Agent", "FAForever-Replay-Viewer");
});

var app = builder.Build();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseRouting();

app.MapGet("/api/replay/{replayId}", async (string replayId, IHttpClientFactory httpClientFactory) =>
{
    if (!replayId.All(char.IsDigit))
        return Results.BadRequest("Invalid replay ID");

    var client = httpClientFactory.CreateClient("FAForeverReplay");
    try
    {
        var response = await client.GetAsync(replayId);
        if (!response.IsSuccessStatusCode)
            return Results.StatusCode((int)response.StatusCode);

        var content = await response.Content.ReadAsByteArrayAsync();
        var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
        return Results.File(content, contentType);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Failed to fetch replay: {ex.Message}");
    }
});

app.MapFallbackToFile("index.html");

app.Run();