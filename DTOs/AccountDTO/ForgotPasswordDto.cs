using System.ComponentModel.DataAnnotations;

namespace ArcadeProject.DTOs.AccountDTO;
public class ForgotPasswordDto
{
    [Required(ErrorMessage = "Email jest wymagany")]
    [EmailAddress(ErrorMessage = "Nieprawidłowy format email")]
    public string Email { get; init; } = string.Empty;
}