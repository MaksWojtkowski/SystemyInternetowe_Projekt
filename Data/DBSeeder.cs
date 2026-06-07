using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ArcadeProject.Models;

namespace ArcadeProject.Data;

public static class DbSeeder
{
    private static readonly Random Rng = new();

    public static async Task SeedAsync(
        AppDbContext db,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration config,
        ILogger logger)
    {
        await SeedRolesAsync(roleManager, logger);
        await SeedAdminAsync(userManager, config, logger);
        await SeedGamesAsync(db, logger);

        if (!db.GameSessions.Any())
        {
            await SeedUsersAndSessionsAsync(db, userManager, logger);
        }
    }

    private static async Task SeedRolesAsync(
        RoleManager<IdentityRole> roleManager, ILogger logger)
    {
        foreach (var role in new[] { "Admin", "User" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Seeder: utworzono rolę {Role}", role);
            }
        }
    }

    private static async Task SeedAdminAsync(
        UserManager<User> userManager, IConfiguration config, ILogger logger)
    {
        var username = "admin";
        var email = "admin@arcadeportal.pl";
        var password = "Admin123!";

        var existing = await userManager.FindByNameAsync(username);

        if (existing is not null)
        {
            if (!await userManager.IsInRoleAsync(existing, "Admin"))
            {
                await userManager.AddToRoleAsync(existing, "Admin");
                logger.LogWarning(
                    "Seeder: konto '{Username}' istniało bez roli Admin — naprawiono.", username);
            }
            else
            {
                logger.LogDebug("Seeder: konto '{Username}' już istnieje, pomijam.", username);
            }

            return;
        }

        var admin = new User
        {
            UserName = username,
            Email = email,
            CreatedAt = DateTime.UtcNow,
        };

        var result = await userManager.CreateAsync(admin, password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
            logger.LogInformation(
                "Seeder: utworzono konto admina '{Username}' ({Email})", username, email);
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogError("Seeder: nie udało się utworzyć admina — {Errors}", errors);
        }
    }

    private static async Task SeedGamesAsync(AppDbContext db, ILogger logger)
    {
        if (db.Games.Any()) return;

        var snake = new Game
        {
            Slug = "snake",
            Title = "Snake",
            Description = "Klasyczna gra węża. Zjedz jak najwięcej i nie wpadnij w ścianę!",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ThumbnailUrl = "https://images.igdb.com/igdb/image/upload/t_cover_big/co6rbq.webp",
        };

        snake.Achievements.Add(new Achievement
        {
            Name = "Pierwsza krew", Description = "Zdobądź pierwszy punkt.", IconUrl = "🐍", Condition = "score>=10"
        });
        snake.Achievements.Add(new Achievement
        {
            Name = "Początkujący", Description = "Zdobądź 50 punktów w jednej grze.", IconUrl = "⭐",
            Condition = "score>=50"
        });
        snake.Achievements.Add(new Achievement
            { Name = "Zaawansowany", Description = "Zdobądź 100 punktów.", IconUrl = "🌟", Condition = "score>=100" });
        snake.Achievements.Add(new Achievement
            { Name = "Mistrz węża", Description = "Zdobądź 200 punktów.", IconUrl = "👑", Condition = "score>=200" });
        snake.Achievements.Add(new Achievement
            { Name = "Legenda", Description = "Zdobądź 500 punktów.", IconUrl = "🏆", Condition = "score>=500" });
        snake.Achievements.Add(new Achievement
        {
            Name = "Wytrwały", Description = "Graj przez ponad 60 sekund.", IconUrl = "⏱️", Condition = "duration>=60"
        });

        db.Games.Add(snake);

        db.Games.Add(new Game
        {
            Slug = "tetris", Title = "Tetris", IsActive = true, CreatedAt = DateTime.UtcNow,
            Description = "Układaj klocki i usuwaj pełne rzędy. Im szybciej, tym lepiej.",
            ThumbnailUrl = "https://images.igdb.com/igdb/image/upload/t_cover_big/co3a78.webp"
        });
        db.Games.Add(new Game
        {
            Slug = "flappy", Title = "Flappy Bird", IsActive = true, CreatedAt = DateTime.UtcNow,
            Description = "Przelec przez jak najwięcej rur. Jeden błąd i koniec!",
            ThumbnailUrl = "https://images.igdb.com/igdb/image/upload/t_cover_big/co9nda.webp"
        });
        db.Games.Add(new Game
        {
            Slug = "pacman", Title = "Pac-Man", IsActive = true, CreatedAt = DateTime.UtcNow,
            Description = "Zjedz wszystkie kulki i uciekaj przed duchami!",
            ThumbnailUrl = "https://images.igdb.com/igdb/image/upload/t_cover_big/co7opo.webp"
        });
        db.Games.Add(new Game
        {
            Slug = "blackjack", Title = "Blackjack", IsActive = true, CreatedAt = DateTime.UtcNow,
            Description = "Dobierz karty tak żeby suma wyniosła jak najbliżej 21.",
            ThumbnailUrl = "https://images.igdb.com/igdb/image/upload/t_cover_big/co9ygq.webp"
        });

        await db.SaveChangesAsync();
        logger.LogInformation("Seeder: dodano gry i achievementy Snake");
    }

    private static async Task SeedUsersAndSessionsAsync(
    AppDbContext db, UserManager<User> userManager, ILogger logger)
{
    var usernames = new[]
    {
        "ArcadeKing", "PixelHunter", "RetroGamer", "NeonJack",
        "ByteWizard",  "GhostRider",  "CyberNova",  "StarBlast",
        "DarkPulse",   "QuickFox",
    };

    var games = await db.Games.ToListAsync();
    if (!games.Any()) return;

    var seededUsers = new List<User>();
    
    foreach (var name in usernames)
    {
        var existing = await userManager.FindByNameAsync(name);
        if (existing is not null)
        {
            seededUsers.Add(existing);
            continue;
        }

        var user = new User
        {
            UserName  = name,
            Email     = $"{name.ToLower()}@arcade.test",
            CreatedAt = DateTime.UtcNow.AddDays(-Rng.Next(1, 180)),
        };

        var result = await userManager.CreateAsync(user, "Haslo123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "User");
            seededUsers.Add(user);
        }
    }

    logger.LogInformation("Seeder: utworzono {Count} użytkowników testowych", seededUsers.Count);
    
    var sessions = new List<GameSession>();

    foreach (var user in seededUsers)
    {
        foreach (var game in games)
        {
            int sessionCount = Rng.Next(3, 21);
            
            int maxScore = game.Slug switch
            {
                "snake"     => 500,
                "tetris"    => 8000,
                "flappy"    => 30,
                "pacman"    => 3000,
                "blackjack" => 10,
                _           => 1000,
            };

            for (int i = 0; i < sessionCount; i++)
            {
                double progress = (double)i / sessionCount;
                int score = (int)(Rng.Next(maxScore / 10, maxScore) * (0.3 + 0.7 * progress));
                score = Math.Max(score, 0);

                sessions.Add(new GameSession
                {
                    UserId          = user.Id,
                    GameId          = game.Id,
                    Score           = score,
                    DurationSeconds = Rng.Next(15, 300),
                    PlayedAt        = DateTime.UtcNow
                        .AddDays(-Rng.Next(0, 90))
                        .AddHours(-Rng.Next(0, 24))
                        .AddMinutes(-Rng.Next(0, 60)),
                });
            }
        }
    }

    db.GameSessions.AddRange(sessions);
    await db.SaveChangesAsync();

    logger.LogInformation(
        "Seeder: dodano {Sessions} losowych sesji dla {Users} użytkowników",
        sessions.Count, seededUsers.Count);
    
    await SeedAchievementsAsync(db, seededUsers, games, logger);
}
    
    private static async Task SeedAchievementsAsync(
    AppDbContext db, List<User> users, List<Game> games, ILogger logger)
    {
    var achievements = await db.Achievements.ToListAsync();
    if (!achievements.Any()) return;

    var userAchievements = new List<UserAchievement>();

    foreach (var user in users)
    {
        foreach (var game in games)
        {
            var bestSession = await db.GameSessions
                .Where(s => s.UserId == user.Id && s.GameId == game.Id)
                .OrderByDescending(s => s.Score)
                .FirstOrDefaultAsync();

            if (bestSession is null) continue;

            var longestSession = await db.GameSessions
                .Where(s => s.UserId == user.Id && s.GameId == game.Id)
                .OrderByDescending(s => s.DurationSeconds)
                .FirstOrDefaultAsync();

            var gameAchievements = achievements.Where(a => a.GameId == game.Id);

            foreach (var achievement in gameAchievements)
            {
                bool earned = achievement.Condition switch
                {
                    var c when c.StartsWith("score>=")    => bestSession.Score >= int.Parse(c[7..]),
                    var c when c.StartsWith("duration>=") => (longestSession?.DurationSeconds ?? 0) >= int.Parse(c[10..]),
                    _ => false
                };

                if (earned)
                {
                    bool alreadyHas = userAchievements
                        .Any(ua => ua.UserId == user.Id && ua.AchievementId == achievement.Id);

                    if (!alreadyHas)
                    {
                        userAchievements.Add(new UserAchievement
                        {
                            UserId        = user.Id,
                            AchievementId = achievement.Id,
                            UnlockedAt    = DateTime.UtcNow.AddDays(-Rng.Next(0, 60)),
                        });
                    }
                }
            }
        }
    }

    db.UserAchievements.AddRange(userAchievements);
    await db.SaveChangesAsync();

    logger.LogInformation("Seeder: przyznano {Count} odznak", userAchievements.Count);
    }
}
