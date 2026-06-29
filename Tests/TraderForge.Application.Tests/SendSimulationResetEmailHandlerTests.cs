using MediatR;
using Moq;
using TraderForge.Application.Events;
using TraderForge.Application.EventHandlers;
using TraderForge.Application.Interfaces.Email;
using TraderForge.Application.Models.Email;

namespace TraderForge.Application.Tests;

public class SendSimulationResetEmailHandlerTests
{
    private readonly Mock<IEmailTemplateService> _templateMock;
    private readonly Mock<IEmailService> _emailMock;
    private readonly SendSimulationResetEmailHandler _handler;

    public SendSimulationResetEmailHandlerTests()
    {
        _templateMock = new Mock<IEmailTemplateService>();
        _emailMock = new Mock<IEmailService>();
        _handler = new SendSimulationResetEmailHandler(_templateMock.Object, _emailMock.Object);
    }

    [Fact]
    public async Task Handle_CreatesRestartMailAndQueuesIt()
    {
        var email = new EmailMessage { To = "user@test.com", Subject = "Reset", Body = "body" };
        _templateMock.Setup(t => t.CreateRestartSimulationMail("user@test.com", "TestUser", 50000m)).Returns(email);

        await _handler.Handle(new SimulationResetEvent("user@test.com", "TestUser", 50000m), CancellationToken.None);

        _templateMock.Verify(t => t.CreateRestartSimulationMail("user@test.com", "TestUser", 50000m), Times.Once);
        _emailMock.Verify(e => e.QueueEmailAsync(email, CancellationToken.None), Times.Once);
    }
}
