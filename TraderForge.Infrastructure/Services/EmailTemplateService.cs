using System;
using System.IO;
using TraderForge.Application.Interfaces.Email;
using TraderForge.Application.Models.Email;

namespace TraderForge.Infrastructure.Services.Email;

public class EmailTemplateService : IEmailTemplateService
{
    private readonly string _basePath = AppDomain.CurrentDomain.BaseDirectory;
    
    private string LoadHtmlTemplate(string fileName)
    {
        string path = Path.Combine(_basePath, "Templates", fileName);
        return File.ReadAllText(path);
    }
    
    private EmailMessage BuildMessage(string destination, string subject, string htmlBody)
    {
        return new EmailMessage
        {
            To = destination,
            Subject = subject,
            Body = htmlBody,
            IsHtml = true
        };
    }

    public EmailMessage CreateWelcomeMail(string destination, string username)
    {
        string html = LoadHtmlTemplate("WelcomeEmail.html")
            .Replace("{{Name}}", username);

        return BuildMessage(destination, "Welcome to Trading Forge!", html);
    }

    public EmailMessage CreateCancellationMail(string destination, string username)
    {
        string html = LoadHtmlTemplate("SubscriptionCancelled.html")
            .Replace("{{Name}}", username);

        return BuildMessage(destination, "We hope to see you back soon", html);
    }
    
    public EmailMessage CreateRestartSimulationMail(string destination, string username, decimal balanceRestored)
    {
        string html = LoadHtmlTemplate("SimulationResetEmail.html")
            .Replace("{{Name}}", username)
            .Replace("{{Balance}}", balanceRestored.ToString("C0"));

        return BuildMessage(destination, "Simulation Restarted", html);
    }
}