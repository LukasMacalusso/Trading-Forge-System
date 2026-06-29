using System.Threading.Channels;
using TraderForge.Application.Interfaces.Email;
using TraderForge.Application.Models.Email;

namespace TraderForge.Infrastructure.Services;

public class EmailChannel: IEmailService
{
    private readonly ChannelWriter<EmailMessage> _channelWriter;

    public EmailChannel(ChannelWriter<EmailMessage> channelWriter)
    {
        _channelWriter = channelWriter;
    }
        
    public async ValueTask QueueEmailAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        await _channelWriter.WriteAsync(message, cancellationToken);
    }
}