using System.ComponentModel.DataAnnotations;

namespace ArcadeProject.DTOs.AdminDTO;
public class UpsertGameDto
{
    public int? Id { get; init; }

    [Required(ErrorMessage = "Slug jest wymagany")]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Tylko małe litery, cyfry i myślniki")]
    [MaxLength(50)]
    public string Slug  { get; init; } = string.Empty;

    [Required(ErrorMessage = "Tytuł jest wymagany")]
    [MaxLength(100)]
    public string Title { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? Description  { get; init; }

    [Url(ErrorMessage = "Nieprawidłowy URL")]
    public string? ThumbnailUrl { get; init; }

    public bool IsActive { get; init; } = true;
}