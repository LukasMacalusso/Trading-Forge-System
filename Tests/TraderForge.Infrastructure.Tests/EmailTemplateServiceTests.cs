using Moq;
using TraderForge.Application.Interfaces.Email;
using TraderForge.Application.Models.Email;
using TraderForge.Infrastructure.Services.Email;

namespace TraderForge.Infrastructure.Tests;

public class EmailTemplateServiceTests
{
    private readonly EmailTemplateService _service;

    public EmailTemplateServiceTests()
    {
        _service = new EmailTemplateService();
    }

    [Fact]
    public void CreateWelcomeMail_ReturnsHtmlEmailWithLayout()
    {
        var result = _service.CreateWelcomeMail("user@test.com", "TestUser");

        Assert.Equal("user@test.com", result.To);
        Assert.Equal("Welcome to Trading Forge!", result.Subject);
        Assert.True(result.IsHtml);
        Assert.Contains("¡Hola, TestUser!", result.Body);
        Assert.Contains("Tu cuenta en", result.Body);
        Assert.Contains("Todos los derechos reservados", result.Body);
        Assert.Contains("bgcolor=\"#2c3e50\"", result.Body.Replace("background-color: #2c3e50", "bgcolor=\"#2c3e50\""));
    }

    [Fact]
    public void CreateCancellationMail_ReturnsHtmlEmailWithLayout()
    {
        var result = _service.CreateCancellationMail("user@test.com", "TestUser");

        Assert.Equal("user@test.com", result.To);
        Assert.Equal("We hope to see you back soon", result.Subject);
        Assert.True(result.IsHtml);
        Assert.Contains("Lamentamos verte partir", result.Body);
        Assert.Contains("Hola <strong>TestUser</strong>", result.Body);
        Assert.Contains("Todos los derechos reservados", result.Body);
    }

    [Fact]
    public void CreateRestartSimulationMail_ReturnsHtmlEmailWithLayout()
    {
        var result = _service.CreateRestartSimulationMail("user@test.com", "TestUser", 50000m);

        Assert.Equal("user@test.com", result.To);
        Assert.Equal("Simulation Restarted", result.Subject);
        Assert.True(result.IsHtml);
        Assert.Contains("¡Simulador Reiniciado!", result.Body);
        Assert.Contains("Hola <strong>TestUser</strong>", result.Body);
        Assert.Contains("50", result.Body);
        Assert.Contains("Todos los derechos reservados", result.Body);
        Assert.Contains("background-color: #e74c3c", result.Body);
        Assert.Contains("color: #27ae60", result.Body);
    }
}
