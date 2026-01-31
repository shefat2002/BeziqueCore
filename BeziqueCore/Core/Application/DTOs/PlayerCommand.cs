namespace BeziqueCore.Core.Application.DTOs;

public record PlayerCommand
{
    public required string UserId { get; init; }
    public required string Action { get; init; }
    public required object Payload { get; init; }
    public string? CommandId { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
