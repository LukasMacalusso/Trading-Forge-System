using MediatR;

namespace TraderForge.Application.Events;

public record SimulationResetEvent(string Email, string Username, decimal BalanceRestored) : INotification;
