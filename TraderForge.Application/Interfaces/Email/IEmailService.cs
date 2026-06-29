using TraderForge.Application.Models.Email;

namespace TraderForge.Application.Interfaces.Email;

public interface IEmailService
{
    ValueTask QueueEmailAsync(EmailMessage message, CancellationToken cancellationToken = default);
}