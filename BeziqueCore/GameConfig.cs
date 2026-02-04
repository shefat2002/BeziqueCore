namespace BeziqueCore;

public sealed class GameConfig
{
    public byte PlayerCount { get; init; } = 2;
    public GameMode Mode { get; init; } = GameMode.Standard;
    public ushort TargetScore { get; init; } = 1500;
    public byte DeckCount { get; init; } = 4;

    public static GameConfig Standard => new();
    public static GameConfig Advanced => new() { Mode = GameMode.Advanced };
    public static GameConfig Custom(byte playerCount, GameMode mode, ushort targetScore, byte deckCount = 4)
        => new() { PlayerCount = playerCount, Mode = mode, TargetScore = targetScore, DeckCount = deckCount };
}

public enum GameMode : byte
{
    Standard,
    Advanced
}
