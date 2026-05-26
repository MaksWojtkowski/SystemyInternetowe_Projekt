using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ArcadeProject.Models;

namespace ArcadeProject.Data;

public class AppDbContext : IdentityDbContext<User>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<Game>            Games            => Set<Game>();
    public DbSet<GameSession>     GameSessions     => Set<GameSession>();
    public DbSet<Achievement>     Achievements     => Set<Achievement>();
}