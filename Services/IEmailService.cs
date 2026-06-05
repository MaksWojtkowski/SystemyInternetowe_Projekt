namespace ArcadeProject.Services;
public interface IEmailService
{
    Task SendWelcomeAsync(string toEmail, string username);
    Task SendPasswordResetAsync(string toEmail, string username, string resetLink);
    Task SendAchievementAsync(string toEmail, string username, string achievementName);
}