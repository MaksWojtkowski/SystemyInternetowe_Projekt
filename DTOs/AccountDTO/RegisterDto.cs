using System.ComponentModel.DataAnnotations;

namespace ArcadeProject.DTOs.AccountDTO;
public class RegisterDto
{
    [Required(ErrorMessage = "Nazwa użytkownika jest wymagana")]
    [MinLength(3, ErrorMessage = "Minimum 3 znaki")]
    [MaxLength(30, ErrorMessage = "Maksimum 30 znaków")]
    public string Username { get; init; } = string.Empty;

    [Required(ErrorMessage = "Email jest wymagany")]
    [EmailAddress(ErrorMessage = "Nieprawidłowy format email")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Hasło jest wymagane")]
    [MinLength(6, ErrorMessage = "Minimum 6 znaków")]
    public string Password { get; init; } = string.Empty;

    [Compare(nameof(Password), ErrorMessage = "Hasła się nie zgadzają")]
    public string ConfirmPassword { get; init; } = string.Empty;
}