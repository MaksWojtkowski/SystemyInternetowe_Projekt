using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using ArcadeProject.Data;
using ArcadeProject.Models;
using ArcadeProject.Services;

var builder = WebApplication.CreateBuilder(args);

// ── SQLite ────────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Identity ──────────────────────────────────────────────────────────────────
builder.Services.AddIdentity<User, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount   = false;
        options.Password.RequiredLength         = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(5);
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath        = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
});

// ── Localization ─────────────────────────────────────────────────────────────
builder.Services.AddLocalization(o => o.ResourcesPath = "Resources");

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supported = new[] { "pl", "en" };
    options.SetDefaultCulture("pl")
        .AddSupportedCultures(supported)
        .AddSupportedUICultures(supported);
    options.RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new CookieRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider()
    };
});

// ── Achievements ─────────────────────────────────────────────────────────────────────
builder.Services.AddScoped<AchievementService>();

// ── Cache ─────────────────────────────────────────────────────────────────────
builder.Services.AddMemoryCache();

// ── MailKit —──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IEmailService, MailKitEmailService>();

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

var app = builder.Build();

// ── Migrations ────────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db          = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var config      = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var logger      = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    db.Database.Migrate();
    
    await DbSeeder.SeedAsync(db, userManager, roleManager, config, logger);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRequestLocalization();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();