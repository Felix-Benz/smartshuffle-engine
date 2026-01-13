using Microsoft.EntityFrameworkCore;
using SmartShuffle.Api.Data;
using SmartShuffle.Api.Dtos;
using SmartShuffle.Api.Models;
using SmartShuffle.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ShuffleService>();

builder.Services.AddDbContext<SmartShuffleDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SmartShuffleDb"));
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.MapControllers();

app.MapPost("/shuffle", async (
    ShuffleRequestDto req,
    SmartShuffleDbContext db,
    ShuffleService shuffleService
) =>
{
    if (req.Tracks == null || req.Tracks.Count == 0)
        return Results.BadRequest("tracks must be non-empty");

    // Load recent history for this user
    var recent = await db.RecentPlays
        .Where(r => r.UserKey == req.UserKey)
        .OrderByDescending(r => r.PlayedAtUtc)
        .Take(req.RecencyWindow)
        .Select(r => r.TrackId)
        .ToListAsync();

    var recentSet = new HashSet<string>(recent);

    // Shuffle pipeline
    var stage1 = shuffleService.ShuffleBag(req.Tracks, req.Seed);
    var stage2 = shuffleService.ApplyRecencyBias(stage1, recentSet, req.Seed);
    var (stage3, violations) = shuffleService.EnforceArtistSpacing(stage2, req.ArtistSpacing, req.Seed);

    // Persist this run into RecentPlays (limit writes if huge)
    var now = DateTime.UtcNow;
    var toInsert = stage3.Take(1000).Select(t => new RecentPlay
    {
        UserKey = req.UserKey,
        TrackId = t.Id,
        PlayedAtUtc = now
    }).ToList();

    db.RecentPlays.AddRange(toInsert);
    await db.SaveChangesAsync();

    var metrics = shuffleService.ComputeMetrics(stage3, violations);

    return Results.Ok(new ShuffleResponseDto
    {
        ShuffledTracks = stage3,
        Metrics = metrics
    });
});

app.MapPost("/reset_history/{userKey}", async (string userKey, SmartShuffleDbContext db) =>
{
    var rows = await db.RecentPlays.Where(r => r.UserKey == userKey).ToListAsync();
    db.RecentPlays.RemoveRange(rows);
    await db.SaveChangesAsync();
    return Results.Ok(new { status = "reset", removed = rows.Count });
});

app.Run();
