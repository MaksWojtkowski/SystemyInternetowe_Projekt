using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace ArcadeProject.Services;
public class MailKitEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<MailKitEmailService> _logger;

    public MailKitEmailService(IConfiguration config, ILogger<MailKitEmailService> logger)
    {
        _settings = config.GetSection("Email").Get<EmailSettings>()
                    ?? throw new InvalidOperationException("Brak sekcji 'Email' w appsettings.json");
        _logger   = logger;
    }
    
    public Task SendWelcomeAsync(string toEmail, string username) =>
        SendAsync(
            toEmail,
            subject: "Witaj na ArcadePortal! 🕹️",
            htmlBody: $"""
                <div style="font-family:sans-serif;max-width:520px;margin:auto">
                    <h2 style="color:#00e5ff">Cześć, {username}!</h2>
                    <p>Twoje konto na <strong>ArcadePortal</strong> zostało utworzone.</p>
                    <p>Zaloguj się i zacznij bić rekordy. 🏆</p>
                    <hr style="border-color:#1e1e2e"/>
                    <small style="color:#888">ArcadePortal — projekt zaliczeniowy</small>
                </div>
            """
        );
    
    public Task SendPasswordResetAsync(string toEmail, string username, string resetLink) =>
        SendAsync(
            toEmail,
            subject: "Reset hasła — ArcadePortal",
            htmlBody: $"""
                <div style="font-family:sans-serif;max-width:520px;margin:auto">
                    <h2 style="color:#00e5ff">Reset hasła</h2>
                    <p>Cześć <strong>{username}</strong>,</p>
                    <p>Kliknij poniższy link żeby ustawić nowe hasło (ważny 1 godzinę):</p>
                    <a href="{resetLink}"
                       style="display:inline-block;padding:.75rem 1.5rem;
                              background:#00e5ff;color:#000;border-radius:6px;
                              text-decoration:none;font-weight:bold">
                        Resetuj hasło
                    </a>
                    <p style="color:#888;font-size:.85rem;margin-top:1.5rem">
                        Jeśli to nie Ty — zignoruj tę wiadomość.
                    </p>
                </div>
            """
        );
    
    public Task SendAchievementAsync(string toEmail, string username, string achievementName) =>
        SendAsync(
            toEmail,
            subject: $"Nowa odznaka: {achievementName} 🏅",
            htmlBody: $"""
                <div style="font-family:sans-serif;max-width:520px;margin:auto">
                    <h2 style="color:#ff2d78">Odznaka odblokowana!</h2>
                    <p>Hej <strong>{username}</strong>,</p>
                    <p>Właśnie zdobyłeś odznakę <strong>{achievementName}</strong>. Tak trzymaj!</p>
                </div>
            """
        );
    
    private async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body    = new TextPart("html") { Text = htmlBody };

        try
        {
            using var client = new SmtpClient();
            
            var security = _settings.UseTls
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None;

            await client.ConnectAsync(_settings.Host, _settings.Port, security);
            
            if (!string.IsNullOrEmpty(_settings.Username))
                await client.AuthenticateAsync(_settings.Username, _settings.Password);

            await client.SendAsync(message);
            await client.DisconnectAsync(quit: true);

            _logger.LogInformation("Email wysłany do {Email}: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd wysyłki emaila do {Email}", toEmail);
        }
    }
}

public class EmailSettings
{
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; } = 587;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FromAddress { get; init; } = string.Empty;
    public string FromName { get; init; } = "ArcadePortal";
    public bool UseTls { get; init; } = false;
}
