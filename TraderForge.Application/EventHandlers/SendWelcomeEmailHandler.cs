using MediatR;
using TraderForge.Application.Events;
using TraderForge.Application.Interfaces.Email;

namespace TraderForge.Application.EventHandlers;

public class SendWelcomeEmailHandler : INotificationHandler<TraderRegisteredEvent>
{
    private readonly IEmailTemplateService _templateService;
    private readonly IEmailService _emailService;

    public SendWelcomeEmailHandler(IEmailTemplateService templateService, IEmailService emailService)
    {
        _templateService = templateService;
        _emailService = emailService;
    }

    public async Task Handle(TraderRegisteredEvent notification, CancellationToken cancellationToken)
    {
        var welcomeEmail = _templateService.CreateWelcomeMail(notification.Email, notification.Username);
        await _emailService.QueueEmailAsync(welcomeEmail, cancellationToken);
    }
}
