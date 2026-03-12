using GameStore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Data;

public class GameStoreContext(DbContextOptions<GameStoreContext> options)
    : DbContext(options)
{
    public DbSet<Game> Games => Set<Game>();

    public DbSet<Genre> Genres => Set<Genre>();

    public DbSet<Publisher> Publishers => Set<Publisher>();

    // protected override void OnModelCreating(ModelBuilder modelBuilder)
    // {
    //     // Configure the composite key for GamePublisher
    //     modelBuilder.Entity<GamePublisher>()
    //         .HasKey(gp => new { gp.GameId, gp.PublisherId });

    //     // Configure relationships
    //     modelBuilder.Entity<GamePublisher>()
    //         .HasOne(gp => gp.Game)
    //         .WithMany(g => g.GamePublishers)
    //         .HasForeignKey(gp => gp.GameId);

    //     modelBuilder.Entity<GamePublisher>()
    //         .HasOne(gp => gp.Publisher)
    //         .WithMany(p => p.GamePublishers)
    //         .HasForeignKey(gp => gp.PublisherId);

    //     // Configure Genre (when I change it to many to many)
    //     // modelBuilder.Entity<Genre>()
    //     //     .HasKey(gg => new { gg.GameId, gg.GenreId });
    // }
}
