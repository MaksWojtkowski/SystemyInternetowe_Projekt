using System.ComponentModel.DataAnnotations;

namespace ArcadeProject.DTOs.ForumDTO;

public class CreateThreadDto
{
    [Required(ErrorMessage = "Tytuł jest wymagany")]
    [MinLength(5,  ErrorMessage = "Minimum 5 znaków")]
    [MaxLength(200, ErrorMessage = "Maksimum 200 znaków")]
    public string Title { get; init; } = string.Empty;

    [Required(ErrorMessage = "Treść jest wymagana")]
    [MinLength(10, ErrorMessage = "Minimum 10 znaków")]
    [MaxLength(5000, ErrorMessage = "Maksimum 5000 znaków")]
    public string Body  { get; init; } = string.Empty;
    
    public int? GameId  { get; init; }
}