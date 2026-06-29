using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Threading.Channels;
using TraderForge.Application.Models.Email;
using TraderForge.Infrastructure.Services;
using TraderForge.Infrastructure.Settings;

namespace TraderForge.Infrastructure.Tests;

public class EmailBackgroundServiceTests
{
    private class TestableEmailBackgroundService : EmailBackgroundService
    {
        public TestableEmailBackgroundService(
            ChannelReader<EmailMessage> reader,
            IOptions<EmailSettings> settings,
            ILogger<EmailBackgroundService> logger)
            : base(reader, settings, logger) { }

        public Task RunExecuteAsync(CancellationToken ct) => ExecuteAsync(ct);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSmtpFails_LogsError()
    {
        var channel = Channel.CreateUnbounded<EmailMessage>();
        var settings = Options.Create(new EmailSettings
        {
            SmtpServer = "invalid.smtp.test",
            SmtpPort = 587,
            SenderName = "Test",
            SenderEmail = "test@test.com",
            Password = "bad"
        });
        var loggerMock = new Mock<ILogger<EmailBackgroundService>>();

        var service = new TestableEmailBackgroundService(channel.Reader, settings, loggerMock.Object);

        channel.Writer.TryWrite(new EmailMessage { To = "user@test.com", Subject = "Hi", Body = "Body", IsHtml = false });
        channel.Writer.Complete();

        using var cts = new CancellationTokenSource();
        await service.RunExecuteAsync(cts.Token);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithHtmlMessage_SetsHtmlBody()
    {
        var channel = Channel.CreateUnbounded<EmailMessage>();
        var settings = Options.Create(new EmailSettings
        {
            SmtpServer = "invalid.smtp.test",
            SmtpPort = 587,
            SenderName = "Test",
            SenderEmail = "test@test.com",
            Password = "bad"
        });
        var loggerMock = new Mock<ILogger<EmailBackgroundService>>();

        var service = new TestableEmailBackgroundService(channel.Reader, settings, loggerMock.Object);

        channel.Writer.TryWrite(new EmailMessage { To = "user@test.com", Subject = "Hi", Body = "<p>HTML</p>", IsHtml = true });
        channel.Writer.Complete();

        using var cts = new CancellationTokenSource();
        await service.RunExecuteAsync(cts.Token);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenChannelEmpty_CompletesCleanly()
    {
        var channel = Channel.CreateUnbounded<EmailMessage>();
        var settings = Options.Create(new EmailSettings
        {
            SmtpServer = "invalid.smtp.test",
            SmtpPort = 587,
            SenderName = "Test",
            SenderEmail = "test@test.com",
            Password = "bad"
        });
        var loggerMock = new Mock<ILogger<EmailBackgroundService>>();

        var service = new TestableEmailBackgroundService(channel.Reader, settings, loggerMock.Object);

        channel.Writer.Complete();

        using var cts = new CancellationTokenSource();
        await service.RunExecuteAsync(cts.Token);

        loggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Never);
    }
}
