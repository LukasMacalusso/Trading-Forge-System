using System.Threading.Channels;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using TraderForge.Application.Models.Email;
using TraderForge.Infrastructure.Settings;

namespace TraderForge.Infrastructure.Services;

public class EmailBackgroundService : BackgroundService
{
    private readonly ChannelReader<EmailMessage> _channelReader;
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailBackgroundService> _logger;

    public EmailBackgroundService(
        ChannelReader<EmailMessage> channelReader,
        IOptions<EmailSettings> emailSettings,
        ILogger<EmailBackgroundService> logger)
    {
        _channelReader = channelReader;
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in _channelReader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await SendEmailAsync(message, stoppingToken);
                _logger.LogInformation($"Email send {message.To}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending {message.To}");
            }
        }
    }
    private async Task SendEmailAsync(EmailMessage message, CancellationToken stoppingToken)
    {
        var email = new MimeMessage();

        email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
        email.To.Add(MailboxAddress.Parse(message.To));
        email.Subject = message.Subject;

        var builder = new BodyBuilder();

        if (message.IsHtml) builder.HtmlBody = message.Body;
        else builder.TextBody = message.Body;

        email.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.StartTls, stoppingToken);
        await smtp.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.Password, stoppingToken);
        await smtp.SendAsync(email, stoppingToken);
        await smtp.DisconnectAsync(true, stoppingToken);
    }
}
