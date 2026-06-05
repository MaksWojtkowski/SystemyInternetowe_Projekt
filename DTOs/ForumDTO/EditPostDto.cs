using System.ComponentModel.DataAnnotations;

namespace ArcadeProject.DTOs.ForumDTO;

public class EditPostDto
{
    public int PostId { get; init; }

    [Required(ErrorMessage = "Treść jest wymagana")]
    [MinLength(2,   ErrorMessage = "Minimum 2 znaki")]
    [MaxLength(5000, ErrorMessage = "Maksimum 5000 znaków")]
    public string Body  { get; init; } = string.Empty;
}