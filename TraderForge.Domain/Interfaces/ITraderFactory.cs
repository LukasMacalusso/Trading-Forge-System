namespace TraderForge.Domain.Interfaces;
using TraderForge.Domain.Entities;

public interface ITraderFactory
{
    Trader CreateWithFreeTrial(string id, string email);
}