namespace SmartShuffle.Api.Models;

public class Track
{
    public int Id { get; set; }               
    public string TrackId { get; set; } = ""; 
    public string Title { get; set; } = "";
    public string Artist { get; set; } = "";
    public string? Album { get; set; }
    public int? DurationMs { get; set; }
}
