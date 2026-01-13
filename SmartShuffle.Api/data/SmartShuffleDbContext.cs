using Microsoft.EntityFrameworkCore;
using SmartShuffle.Api.Models;

namespace SmartShuffle.Api.Data;

public class SmartShuffleDbContext : DbContext
{
    public SmartShuffleDbContext(DbContextOptions<SmartShuffleDbContext> options) : base(options) { }

    public DbSet<Track> Tracks => Set<Track>();
    public DbSet<RecentPlay> RecentPlays => Set<RecentPlay>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Track>()
            .HasIndex(t => t.TrackId);

        modelBuilder.Entity<RecentPlay>()
            .HasIndex(r => new { r.UserKey, r.PlayedAtUtc });

        base.OnModelCreating(modelBuilder);
    }
}
