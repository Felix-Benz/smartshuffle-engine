namespace SmartShuffle.Api.Models;

public class RecentPlay
{
    public int Id { get; set; }
    public string UserKey { get; set; } = "default";
    public string TrackId { get; set; } = "";
    public DateTime PlayedAtUtc { get; set; } = DateTime.UtcNow;
}
