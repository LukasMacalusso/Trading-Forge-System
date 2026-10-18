using MediatR;

namespace TraderForge.Application.Events;

public record SubscriptionCancelledEvent(string Email, string Username) : INotification;
