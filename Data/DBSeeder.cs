using Microsoft.AspNetCore.Identity;
using ArcadeProject.Models;

namespace ArcadeProject.Data;
public static class DbSeeder
{
    public static async Task SeedAsync(
        AppDbContext              db,
        UserManager<User>        userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration           config,
        ILogger                  logger)
    {
        await SeedRolesAsync(roleManager, logger);
        await SeedAdminAsync(userManager, config, logger);
        await SeedGamesAsync(db, logger);
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
        var email    = "admin@arcadeportal.pl";
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
            UserName  = username,
            Email     = email,
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
        Slug        = "snake",
        Title       = "Snake",
        Description = "Klasyczna gra węża. Zjedz jak najwięcej i nie wpadnij w ścianę!",
        IsActive    = true,
        CreatedAt   = DateTime.UtcNow,
    };
    
    snake.Achievements.Add(new Achievement { Name = "Pierwsza krew",  Description = "Zdobądź pierwszy punkt.",           IconUrl = "🐍", Condition = "score>=10"   });
    snake.Achievements.Add(new Achievement { Name = "Początkujący",   Description = "Zdobądź 50 punktów w jednej grze.", IconUrl = "⭐", Condition = "score>=50"   });
    snake.Achievements.Add(new Achievement { Name = "Zaawansowany",   Description = "Zdobądź 100 punktów.",              IconUrl = "🌟", Condition = "score>=100"  });
    snake.Achievements.Add(new Achievement { Name = "Mistrz węża",    Description = "Zdobądź 200 punktów.",              IconUrl = "👑", Condition = "score>=200"  });
    snake.Achievements.Add(new Achievement { Name = "Legenda",        Description = "Zdobądź 500 punktów.",              IconUrl = "🏆", Condition = "score>=500"  });
    snake.Achievements.Add(new Achievement { Name = "Wytrwały",       Description = "Graj przez ponad 60 sekund.",       IconUrl = "⏱️", Condition = "duration>=60" });

    db.Games.Add(snake);

    db.Games.Add(new Game
    {
        Slug        = "tetris",
        Title       = "Tetris",
        Description = "Układaj klocki i usuwaj pełne rzędy. Im szybciej, tym lepiej.",
        IsActive    = true,
        CreatedAt   = DateTime.UtcNow,
    });

    db.Games.Add(new Game
    {
        Slug        = "flappy",
        Title       = "Flappy Bird",
        Description = "Przelec przez jak najwięcej rur. Jeden błąd i koniec!",
        IsActive    = true,
        CreatedAt   = DateTime.UtcNow,
    });

    await db.SaveChangesAsync();
    logger.LogInformation("Seeder: dodano gry i achievementy Snake");
}
}
