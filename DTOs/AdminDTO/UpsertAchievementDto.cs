using System.ComponentModel.DataAnnotations;

namespace ArcadeProject.DTOs.AdminDTO;
public class UpsertAchievementDto
{
    public int? Id     { get; init; }
    public int  GameId { get; init; }

    [Required(ErrorMessage = "Nazwa jest wymagana")]
    [MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    [Required(ErrorMessage = "Opis jest wymagany")]
    [MaxLength(300)]
    public string Description { get; init; } = string.Empty;

    [MaxLength(10)]
    public string? IconUrl { get; init; }

    [Required(ErrorMessage = "Warunek jest wymagany")]
    [RegularExpression(@"^(score|duration)(>=|>)\d+$",
        ErrorMessage = "Format: score>=100 lub duration>=60")]
    public string Condition { get; init; } = string.Empty;
}
