using System;
using System.IO;
using TraderForge.Application.Interfaces.Email;
using TraderForge.Application.Models.Email;

namespace TraderForge.Infrastructure.Services.Email;

public class EmailTemplateService : IEmailTemplateService
{
    private readonly string _basePath = AppDomain.CurrentDomain.BaseDirectory;
    
    private string LoadTemplate(string fileName)
    {
        string path = Path.Combine(_basePath, "Templates", fileName);
        return File.ReadAllText(path);
    }
    
    private EmailMessage BuildFromLayout(string destination, string subject,
        string title, string headerBgColor, string highlightColor,
        string contentHtml, string highlightExtraStyle = "")
    {
        var layout = LoadTemplate("LayoutEmail.html");
        var body = layout
            .Replace("{{Title}}", title)
            .Replace("{{HeaderBgColor}}", headerBgColor)
            .Replace("{{HighlightColor}}", highlightColor)
            .Replace("{{HighlightExtraStyle}}", highlightExtraStyle)
            .Replace("{{Content}}", contentHtml);

        return new EmailMessage
        {
            To = destination,
            Subject = subject,
            Body = body,
            IsHtml = true
        };
    }

    public EmailMessage CreateWelcomeMail(string destination, string username)
    {
        var content = LoadTemplate("WelcomeEmail.html");
        return BuildFromLayout(destination, "Welcome to Trading Forge!",
            $"¡Hola, {username}!", "#2c3e50", "#3498db", content);
    }

    public EmailMessage CreateCancellationMail(string destination, string username)
    {
        var content = LoadTemplate("SubscriptionCancelled.html")
            .Replace("{{Name}}", username);
        return BuildFromLayout(destination, "We hope to see you back soon",
            "Lamentamos verte partir", "#2c3e50", "#e74c3c", content);
    }
    
    public EmailMessage CreateRestartSimulationMail(string destination, string username, decimal balanceRestored)
    {
        var content = LoadTemplate("SimulationResetEmail.html")
            .Replace("{{Name}}", username)
            .Replace("{{Balance}}", balanceRestored.ToString("C0"));
        return BuildFromLayout(destination, "Simulation Restarted",
            "¡Simulador Reiniciado!", "#e74c3c", "#27ae60", content, "font-size:1.2em;");
    }
}