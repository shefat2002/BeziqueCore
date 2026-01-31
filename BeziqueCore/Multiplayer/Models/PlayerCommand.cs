namespace BeziqueCore.Multiplayer.Models;

/// <summary>
/// Represents a command from a player in a multiplayer game.
/// Used for network transmission of player actions.
/// </summary>
public record PlayerCommand
{
    /// <summary>
    /// The unique identifier of the player issuing the command.
    /// </summary>
    public required string UserId { get; init; }

    /// <summary>
    /// The action to perform (e.g., "PlayCard", "DeclareMeld", "SkipMeld", "SwitchSevenOfTrump").
    /// </summary>
    public required string Action { get; init; }

    /// <summary>
    /// The payload containing action-specific data.
    /// </summary>
    public required object Payload { get; init; }

    /// <summary>
    /// Optional command ID for idempotency and duplicate detection.
    /// </summary>
    public string? CommandId { get; init; }

    /// <summary>
    /// Timestamp when the command was created (client-side).
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
