using TraderForge.Infrastructure.Settings;

namespace TraderForge.Infrastructure.Tests;

public class EmailSettingsTests
{
    [Fact]
    public void Properties_RoundTrip()
    {
        var pwd = Guid.NewGuid().ToString("N");
        var settings = new EmailSettings
        {
            SmtpServer = "smtp.test.com",
            SmtpPort = 587,
            SenderName = "Test",
            SenderEmail = "test@test.com",
            Password = pwd
        };

        Assert.Equal("smtp.test.com", settings.SmtpServer);
        Assert.Equal(587, settings.SmtpPort);
        Assert.Equal("Test", settings.SenderName);
        Assert.Equal("test@test.com", settings.SenderEmail);
        Assert.Equal(pwd, settings.Password);
    }

    [Fact]
    public void DefaultValues_AreEmpty()
    {
        var settings = new EmailSettings();

        Assert.Equal(string.Empty, settings.SmtpServer);
        Assert.Equal(0, settings.SmtpPort);
        Assert.Equal(string.Empty, settings.SenderName);
        Assert.Equal(string.Empty, settings.SenderEmail);
        Assert.Equal(string.Empty, settings.Password);
    }
}
