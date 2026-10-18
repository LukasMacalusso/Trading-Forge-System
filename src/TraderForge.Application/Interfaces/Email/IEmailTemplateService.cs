using TraderForge.Application.Models.Email;

namespace TraderForge.Application.Interfaces.Email;

public interface IEmailTemplateService
{
    EmailMessage CreateWelcomeMail(string destination, string username);
    EmailMessage CreateCancellationMail(string destination, string username);
    EmailMessage CreateRestartSimulationMail(string destination, string username, decimal balanceRestored);
}
