using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ArcadeProject.Models;

namespace ArcadeProject.Data;

public class AppDbContext : IdentityDbContext<User>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<Game> Games => Set<Game>();
    public DbSet<GameSession> GameSessions => Set<GameSession>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();
    public DbSet<Models.Thread> Threads => Set<Models.Thread>();
    public DbSet<Post> Posts => Set<Post>();
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
    base.OnModelCreating(builder);

    // Game
    builder.Entity<Game>(e =>
    {
        e.HasIndex(g => g.Slug).IsUnique();
    });

    // GameSession
    builder.Entity<GameSession>(e =>
    {
        e.HasOne(s => s.User)
         .WithMany(u => u.GameSessions)
         .HasForeignKey(s => s.UserId)
         .OnDelete(DeleteBehavior.Cascade);

        e.HasOne(s => s.Game)
         .WithMany(g => g.GameSessions)
         .HasForeignKey(s => s.GameId)
         .OnDelete(DeleteBehavior.Cascade);
    });

    // Achievement
    builder.Entity<Achievement>(e =>
    {
        e.HasOne(a => a.Game)
         .WithMany(g => g.Achievements)
         .HasForeignKey(a => a.GameId)
         .OnDelete(DeleteBehavior.Cascade);
    });

    // UserAchievement
    builder.Entity<UserAchievement>(e =>
    {
        e.HasIndex(ua => new { ua.UserId, ua.AchievementId }).IsUnique();

        e.HasOne(ua => ua.User)
         .WithMany(u => u.UserAchievements)
         .HasForeignKey(ua => ua.UserId)
         .OnDelete(DeleteBehavior.Cascade);

        e.HasOne(ua => ua.Achievement)
         .WithMany(a => a.UserAchievements)
         .HasForeignKey(ua => ua.AchievementId)
         .OnDelete(DeleteBehavior.Cascade);
    });

    // Thread
    builder.Entity<Models.Thread>(e =>
    {
        e.HasOne(t => t.User)
         .WithMany(u => u.Threads)
         .HasForeignKey(t => t.UserId)
         .OnDelete(DeleteBehavior.Cascade);

        e.HasOne(t => t.Game)
         .WithMany(g => g.Threads)
         .HasForeignKey(t => t.GameId)
         .IsRequired(false)
         .OnDelete(DeleteBehavior.SetNull);
    });

    // Post
    builder.Entity<Post>(e =>
    {
        e.HasOne(p => p.Thread)
         .WithMany(t => t.Posts)
         .HasForeignKey(p => p.ThreadId)
         .OnDelete(DeleteBehavior.Cascade);

        e.HasOne(p => p.User)
         .WithMany(u => u.Posts)
         .HasForeignKey(p => p.UserId)
         .OnDelete(DeleteBehavior.Cascade);
    });
    }
}