using MediatR;
using Moq;
using TraderForge.Application.Events;
using TraderForge.Application.EventHandlers;
using TraderForge.Application.Interfaces.Email;
using TraderForge.Application.Models.Email;

namespace TraderForge.Application.Tests;

public class SendSubscriptionCancelledEmailHandlerTests
{
    private readonly Mock<IEmailTemplateService> _templateMock;
    private readonly Mock<IEmailService> _emailMock;
    private readonly SendSubscriptionCancelledEmailHandler _handler;

    public SendSubscriptionCancelledEmailHandlerTests()
    {
        _templateMock = new Mock<IEmailTemplateService>();
        _emailMock = new Mock<IEmailService>();
        _handler = new SendSubscriptionCancelledEmailHandler(_templateMock.Object, _emailMock.Object);
    }

    [Fact]
    public async Task Handle_CreatesCancellationMailAndQueuesIt()
    {
        var email = new EmailMessage { To = "user@test.com", Subject = "Cancelled", Body = "body" };
        _templateMock.Setup(t => t.CreateCancellationMail("user@test.com", "TestUser")).Returns(email);

        await _handler.Handle(new SubscriptionCancelledEvent("user@test.com", "TestUser"), CancellationToken.None);

        _templateMock.Verify(t => t.CreateCancellationMail("user@test.com", "TestUser"), Times.Once);
        _emailMock.Verify(e => e.QueueEmailAsync(email, CancellationToken.None), Times.Once);
    }
}
