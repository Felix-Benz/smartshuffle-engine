namespace SmartShuffle.Api.Dtos;

public class ShuffleRequestDto
{
    public int? Seed { get; set; }
    public int RecencyWindow { get; set; } = 200;
    public int ArtistSpacing { get; set; } = 8;
    public string UserKey { get; set; } = "default";
    public List<TrackDto> Tracks { get; set; } = new();
}

public class TrackDto
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Artist { get; set; } = "";
    public string? Album { get; set; }
    public int? DurationMs { get; set; }
}
