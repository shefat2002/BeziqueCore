namespace BeziqueCore.Multiplayer.Models;

/// <summary>
/// Represents the result of a game action/command execution.
/// </summary>
public record GameActionResult
{
    /// <summary>
    /// Indicates whether the action was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Error message if the action failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Optional data returned by the action.
    /// </summary>
    public object? Data { get; init; }

    /// <summary>
    /// The state of the game after this action.
    /// </summary>
    public string? GameState { get; init; }

    /// <summary>
    /// The user ID of the next player to act.
    /// </summary>
    public string? NextPlayerUserId { get; init; }

    /// <summary>
    /// Creates a successful action result.
    /// </summary>
    public static GameActionResult Ok(object? data = null)
    {
        return new GameActionResult
        {
            Success = true,
            Data = data
        };
    }

    /// <summary>
    /// Creates a failed action result with an error message.
    /// </summary>
    public static GameActionResult Error(string errorMessage)
    {
        return new GameActionResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}
