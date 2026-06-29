using System.Threading.Channels;
using Moq;
using TraderForge.Application.Interfaces.Email;
using TraderForge.Application.Models.Email;
using TraderForge.Infrastructure.Services;

namespace TraderForge.Infrastructure.Tests;

public class EmailChannelTests
{
    [Fact]
    public async Task QueueEmailAsync_WritesToChannel()
    {
        var channel = Channel.CreateUnbounded<EmailMessage>();
        var service = new EmailChannel(channel.Writer);

        var message = new EmailMessage { To = "test@test.com", Subject = "Test", Body = "Hello" };
        await service.QueueEmailAsync(message);

        Assert.True(channel.Reader.TryRead(out var result));
        Assert.Equal("test@test.com", result.To);
    }
}
