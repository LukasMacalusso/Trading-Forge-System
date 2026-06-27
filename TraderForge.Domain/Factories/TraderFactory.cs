using TraderForge.Domain.Entities;
namespace TraderForge.Domain.Factories;

public class TraderFactory
{
    public Trader CreateWithFreeTrial(string id, string email)
    {
        return new Trader(id, email)
        {
            UserName = email,
            FreeTrialRegistrationDate = DateTime.UtcNow,
            FreeTrialExpirationDate = DateTime.UtcNow.AddDays(7)
        };
    }
}
