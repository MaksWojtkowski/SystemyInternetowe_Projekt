using System.ComponentModel.DataAnnotations;

namespace ArcadeProject.DTOs.ProfileDTO;
public class EditProfileDto
{
    [Required(ErrorMessage = "Nazwa użytkownika jest wymagana")]
    [MinLength(3,  ErrorMessage = "Minimum 3 znaki")]
    [MaxLength(30, ErrorMessage = "Maksimum 30 znaków")]
    public string Username  { get; init; } = string.Empty;

    [Url(ErrorMessage = "Nieprawidłowy format URL")]
    [MaxLength(500)]
    public string? AvatarUrl { get; init; }
}