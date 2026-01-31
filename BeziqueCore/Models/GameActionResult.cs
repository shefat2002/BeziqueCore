namespace BeziqueCore.Models;

public record GameActionResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public object? Data { get; init; }
    public string? GameState { get; init; }
    public string? NextPlayerUserId { get; init; }

    public static GameActionResult Ok(object? data = null) => new() { Success = true, Data = data };
    public static GameActionResult Error(string errorMessage) => new() { Success = false, ErrorMessage = errorMessage };
}
