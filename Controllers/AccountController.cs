using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ArcadeProject.DTOs;
using ArcadeProject.Models;
using ArcadeProject.Services;

namespace ArcadeProject.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<User>   _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IEmailService       _email;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<User>        userManager,
        SignInManager<User>      signInManager,
        IEmailService            email,
        ILogger<AccountController> logger)
    {
        _userManager   = userManager;
        _signInManager = signInManager;
        _email         = email;
        _logger        = logger;
    }

    // ── REJESTRACJA ───────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        if (await _userManager.FindByNameAsync(dto.Username) is not null)
        {
            ModelState.AddModelError(nameof(dto.Username), "Ta nazwa użytkownika jest już zajęta.");
            return View(dto);
        }

        var user = new User
        {
            UserName  = dto.Username,
            Email     = dto.Email,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
                ModelState.AddModelError(string.Empty, err.Description);
            return View(dto);
        }
        
        await _userManager.AddToRoleAsync(user, "User");
        _logger.LogInformation("Nowy użytkownik: {Username}", dto.Username);
        await _email.SendWelcomeAsync(dto.Email, dto.Username);
        await _signInManager.SignInAsync(user, isPersistent: false);
        TempData["Success"] = $"Witaj, {dto.Username}! Konto zostało utworzone.";
        return RedirectToAction("Index", "Home");
    }

    // ── LOGOWANIE ─────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto dto, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(dto);
        var user = await _userManager.FindByNameAsync(dto.UsernameOrEmail)
                ?? await _userManager.FindByEmailAsync(dto.UsernameOrEmail);

        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Nieprawidłowe dane logowania.");
            return View(dto);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user, dto.Password, dto.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            _logger.LogInformation("Zalogowano: {Username}", user.UserName);
            return LocalRedirect(returnUrl ?? Url.Action("Index", "Home")!);
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Konto zablokowane po zbyt wielu nieudanych próbach. Spróbuj za chwilę.");
            return View(dto);
        }

        ModelState.AddModelError(string.Empty, "Nieprawidłowe dane logowania.");
        return View(dto);
    }

    // ── WYLOGOWANIE ───────────────────────────────────────────────────────────

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    // ── RESET HASŁA — krok 1: formularz email ─────────────────────────────────

    [HttpGet]
    public IActionResult ForgotPassword() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var user = await _userManager.FindByEmailAsync(dto.Email);
        
        if (user is not null)
        {
            var token     = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = Url.Action("ResetPassword", "Account",
                new { userId = user.Id, token }, Request.Scheme)!;

            await _email.SendPasswordResetAsync(dto.Email, user.UserName!, resetLink);
        }

        TempData["Success"] = "Jeśli podany email istnieje w systemie, wysłaliśmy link do resetowania hasła.";
        return RedirectToAction(nameof(Login));
    }

    // ── RESET HASŁA — krok 2: nowe hasło ─────────────────────────────────────

    [HttpGet]
    public IActionResult ResetPassword(string userId, string token)
    {
        var dto = new ResetPasswordDto { UserId = userId, Token = token };
        return View(dto);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user is null)
        {
            TempData["Error"] = "Nieprawidłowy link resetowania.";
            return RedirectToAction(nameof(Login));
        }

        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        if (result.Succeeded)
        {
            TempData["Success"] = "Hasło zostało zmienione. Możesz się teraz zalogować.";
            return RedirectToAction(nameof(Login));
        }

        foreach (var err in result.Errors)
            ModelState.AddModelError(string.Empty, err.Description);
        return View(dto);
    }
}