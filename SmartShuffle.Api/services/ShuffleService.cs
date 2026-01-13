using SmartShuffle.Api.Dtos;

namespace SmartShuffle.Api.Services;

public class ShuffleService
{
    public List<TrackDto> ShuffleBag(List<TrackDto> tracks, int? seed)
    {
        var rng = seed.HasValue ? new Random(seed.Value) : new Random();
        var copy = tracks.ToList();
        // Fisher-Yates shuffle
        for (int i = copy.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (copy[i], copy[j]) = (copy[j], copy[i]);
        }
        return copy;
    }

    public List<TrackDto> ApplyRecencyBias(List<TrackDto> tracks, HashSet<string> recentIds, int? seed)
    {
        var fresh = tracks.Where(t => !recentIds.Contains(t.Id)).ToList();
        var stale = tracks.Where(t => recentIds.Contains(t.Id)).ToList();
        fresh = ShuffleBag(fresh, seed);
        stale = ShuffleBag(stale, seed.HasValue ? seed + 1 : null);
        fresh.AddRange(stale);
        return fresh;
    }

    public (List<TrackDto> Result, int Violations) EnforceArtistSpacing(List<TrackDto> tracks, int artistSpacing, int? seed)
    {
        if (artistSpacing <= 0) return (tracks, 0);

        var rng = seed.HasValue ? new Random(seed.Value + 99) : new Random();
        var remaining = tracks.ToList();
        // randomize a bit before greedy
        remaining = ShuffleBag(remaining, seed.HasValue ? seed + 99 : null);

        var result = new List<TrackDto>(tracks.Count);
        var window = new Queue<string>();
        int violations = 0;

        while (remaining.Count > 0)
        {
            var candidates = remaining
                .Select((t, idx) => (t, idx))
                .Where(x => !window.Contains(x.t.Artist))
                .ToList();

            int pickIndex;
            if (candidates.Count > 0)
            {
                pickIndex = candidates[rng.Next(candidates.Count)].idx;
            }
            else
            {
                pickIndex = rng.Next(remaining.Count);
                violations++;
            }

            var chosen = remaining[pickIndex];
            remaining.RemoveAt(pickIndex);

            result.Add(chosen);

            window.Enqueue(chosen.Artist);
            while (window.Count > artistSpacing) window.Dequeue();
        }

        return (result, violations);
    }

    public Dictionary<string, object> ComputeMetrics(List<TrackDto> tracks, int spacingViolations)
    {
        double clusterScore = 0;
        if (tracks.Count > 1)
        {
            int same = 0;
            for (int i = 1; i < tracks.Count; i++)
                if (tracks[i].Artist == tracks[i - 1].Artist) same++;
            clusterScore = (double)same / (tracks.Count - 1);
        }

        var topArtistShare = tracks.Count == 0 ? 0 : tracks
            .GroupBy(t => t.Artist)
            .Select(g => g.Count())
            .DefaultIfEmpty(0)
            .Max() / (double)tracks.Count;

        return new Dictionary<string, object>
        {
            ["trackCount"] = tracks.Count,
            ["artistClusterScore"] = clusterScore,
            ["topArtistShare"] = topArtistShare,
            ["artistSpacingViolations"] = spacingViolations
        };
    }
}
