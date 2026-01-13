namespace SmartShuffle.Api.Dtos;

public class ShuffleResponseDto
{
    public List<TrackDto> ShuffledTracks { get; set; } = new();
    public Dictionary<string, object> Metrics { get; set; } = new();
}
