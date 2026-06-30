using System;

namespace TraderForge.Application.DTOs;

public class ApprovePendingOperationCommand
{
    public string TraderId { get; set; } = string.Empty;
    public Guid OperationId { get; set; }
}
