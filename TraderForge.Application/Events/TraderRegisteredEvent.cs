using MediatR;
namespace TraderForge.Application.Events;

public record TraderRegisteredEvent(string Email, string Username) : INotification;
