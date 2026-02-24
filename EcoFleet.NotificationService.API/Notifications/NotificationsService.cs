using System.Net;
using System.Net.Mail;
using EcoFleet.NotificationService.API.Notifications.DTOs;

namespace EcoFleet.NotificationService.API.Notifications;

public class NotificationsService : INotificationsService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<NotificationsService> _logger;

    public NotificationsService(IConfiguration configuration, ILogger<NotificationsService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendDriverSuspendedNotification(DriverSuspendedEventDTO eventDTO)
    {
        var subject = "Suspended driver";

        var body = $"""
        Dear {eventDTO.FirstName} {eventDTO.LastName},

        We inform you that your activity as a driver has been suspended.

        EcoFleet Team
        """;

        await SendMessage(eventDTO.Email, subject, body);
    }

    public async Task SendDriverReinstatedNotification(DriverReinstatedEventDTO eventDTO)
    {
        var subject = "Reinstated driver";

        var body = $"""
        Dear {eventDTO.FirstName} {eventDTO.LastName},

        We inform you that your activity as a driver has been reinstated.

        EcoFleet Team
        """;

        await SendMessage(eventDTO.Email, subject, body);
    }

    private async Task SendMessage(string recipientEmail, string subject, string body)
    {
        _logger.LogInformation("Preparing to send email to {Recipient}. Subject: {Subject}", recipientEmail, subject);

        try
        {
            var ourEmail = _configuration.GetValue<string>("EMAIL_CONFIGURATIONS:EMAIL");
            var password = _configuration.GetValue<string>("EMAIL_CONFIGURATIONS:PASSWORD");
            var host = _configuration.GetValue<string>("EMAIL_CONFIGURATIONS:HOST");
            var port = _configuration.GetValue<int>("EMAIL_CONFIGURATIONS:PORT");

            var smtpClient = new SmtpClient(host, port);
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(ourEmail, password);

            var message = new MailMessage(ourEmail!, recipientEmail, subject, body);
            await smtpClient.SendMailAsync(message);

            _logger.LogInformation("Email sent successfully to {Recipient}", recipientEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMTP Error: Failed to send email to {Recipient}", recipientEmail);
            throw;
        }
    }
}
