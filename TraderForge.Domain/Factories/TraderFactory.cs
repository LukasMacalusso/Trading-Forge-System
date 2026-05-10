using TraderForge.Domain.Entities;
using TraderForge.Domain.Interfaces;

public class TraderFactory : ITraderFactory
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