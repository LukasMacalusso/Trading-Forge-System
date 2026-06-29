using MediatR;
using TraderForge.Application.Events;
using TraderForge.Application.Interfaces.Email;

namespace TraderForge.Application.EventHandlers;

public class SendSubscriptionCancelledEmailHandler : INotificationHandler<SubscriptionCancelledEvent>
{
    private readonly IEmailTemplateService _templateService;
    private readonly IEmailService _emailService;

    public SendSubscriptionCancelledEmailHandler(IEmailTemplateService templateService, IEmailService emailService)
    {
        _templateService = templateService;
        _emailService = emailService;
    }

    public async Task Handle(SubscriptionCancelledEvent notification, CancellationToken cancellationToken)
    {
        var email = _templateService.CreateCancellationMail(notification.Email, notification.Username);
        await _emailService.QueueEmailAsync(email, cancellationToken);
    }
}
