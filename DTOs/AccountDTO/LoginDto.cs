using System.ComponentModel.DataAnnotations;

namespace ArcadeProject.DTOs.AccountDTO;
public class LoginDto
{
    [Required(ErrorMessage = "Podaj nazwę użytkownika lub email")]
    public string UsernameOrEmail { get; init; } = string.Empty;

    [Required(ErrorMessage = "Hasło jest wymagane")]
    public string Password { get; init; } = string.Empty;

    public bool RememberMe { get; init; }
}