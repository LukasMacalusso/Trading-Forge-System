using System;

namespace TraderForge.Application.DTOs;

public class RejectPendingOperationCommand
{
    public string TraderId { get; set; } = string.Empty;
    public Guid OperationId { get; set; }
}
