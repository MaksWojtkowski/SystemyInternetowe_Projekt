using System.ComponentModel.DataAnnotations;

namespace ArcadeProject.DTOs.ProfileDTO;
public class ChangePasswordDto
{
    [Required(ErrorMessage = "Podaj obecne hasło")]
    public string CurrentPassword { get; init; } = string.Empty;

    [Required(ErrorMessage = "Podaj nowe hasło")]
    [MinLength(6, ErrorMessage = "Minimum 6 znaków")]
    public string NewPassword { get; init; } = string.Empty;

    [Compare(nameof(NewPassword), ErrorMessage = "Hasła się nie zgadzają")]
    public string ConfirmPassword { get; init; } = string.Empty;
}