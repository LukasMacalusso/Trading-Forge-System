using TraderForge.Domain.Factories;

namespace TraderForge.Domain.Tests;

public class TraderFactoryTests
{
    [Fact]
    public void CreateWithFreeTrial_ShouldSetUserNameAndTrialDates()
    {
        var factory = new TraderFactory();
        var id = Guid.NewGuid().ToString();
        const string email = "test@traderforge.com";

        var trader = factory.CreateWithFreeTrial(id, email);

        Assert.Equal(id, trader.Id);
        Assert.Equal(email, trader.Email);
        Assert.Equal(email, trader.UserName);
        Assert.True(trader.FreeTrialRegistrationDate <= DateTime.UtcNow);
        Assert.True(trader.FreeTrialExpirationDate > DateTime.UtcNow);
        Assert.True((trader.FreeTrialExpirationDate - trader.FreeTrialRegistrationDate).Days >= 7);
    }
}
