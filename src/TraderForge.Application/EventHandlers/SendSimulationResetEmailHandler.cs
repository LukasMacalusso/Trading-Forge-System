using MediatR;
using TraderForge.Application.Events;
using TraderForge.Application.Interfaces.Email;

namespace TraderForge.Application.EventHandlers;

public class SendSimulationResetEmailHandler : INotificationHandler<SimulationResetEvent>
{
    private readonly IEmailTemplateService _templateService;
    private readonly IEmailService _emailService;

    public SendSimulationResetEmailHandler(IEmailTemplateService templateService, IEmailService emailService)
    {
        _templateService = templateService;
        _emailService = emailService;
    }

    public async Task Handle(SimulationResetEvent notification, CancellationToken cancellationToken)
    {
        var email = _templateService.CreateRestartSimulationMail(notification.Email, notification.Username, notification.BalanceRestored);
        await _emailService.QueueEmailAsync(email, cancellationToken);
    }
}
