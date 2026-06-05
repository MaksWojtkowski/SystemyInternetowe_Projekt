using System.ComponentModel.DataAnnotations;

namespace ArcadeProject.DTOs.AccountDTO;
public class ResetPasswordDto
{
    [Required]
    public string UserId { get; init; } = string.Empty;

    [Required]
    public string Token { get; init; } = string.Empty;

    [Required(ErrorMessage = "Hasło jest wymagane")]
    [MinLength(6, ErrorMessage = "Minimum 6 znaków")]
    public string NewPassword { get; init; } = string.Empty;

    [Compare(nameof(NewPassword), ErrorMessage = "Hasła się nie zgadzają")]
    public string ConfirmPassword { get; init; } = string.Empty;
}