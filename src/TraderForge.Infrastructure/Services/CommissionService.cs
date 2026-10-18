using System.Globalization;
using Microsoft.Extensions.Configuration;
using TraderForge.Domain.Services;

namespace TraderForge.Infrastructure.Services;

public class CommissionService : ICommissionService
{
    private readonly decimal _rate;

    public CommissionService(IConfiguration configuration)
    {
        var raw = configuration.GetValue<string>("CommissionSettings:Rate") ?? "0";
        _rate = decimal.Parse(raw, CultureInfo.InvariantCulture) / 100;
    }

    public decimal Calculate(decimal total) => Math.Round(total * _rate, 2);
}
