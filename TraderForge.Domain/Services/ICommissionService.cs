namespace TraderForge.Domain.Services;

public interface ICommissionService
{
    decimal Calculate(decimal total);
}
