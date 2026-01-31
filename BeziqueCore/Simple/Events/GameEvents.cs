namespace BeziqueCore.Simple.Events;

public class GameEventArgs : EventArgs
{
    public string GameState { get; init; } = string.Empty;
}

public class CardPlayedEventArgs : GameEventArgs
{
    public required string PlayerName { get; init; }
    public required string PlayerId { get; init; }
    public required string Card { get; init; }
    public required int CardIndex { get; init; }
}

public class MeldDeclaredEventArgs : GameEventArgs
{
    public required string PlayerName { get; init; }
    public required string PlayerId { get; init; }
    public required string MeldType { get; init; }
    public required int Points { get; init; }
}

public class MeldSkippedEventArgs : GameEventArgs
{
    public required string PlayerName { get; init; }
    public required string PlayerId { get; init; }
}

public class TrickResolvedEventArgs : GameEventArgs
{
    public required string WinnerName { get; init; }
    public required string WinnerId { get; init; }
    public required int Points { get; init; }
}

public class PlayerTurnChangedEventArgs : GameEventArgs
{
    public required string PlayerName { get; init; }
    public required string PlayerId { get; init; }
}

public class RoundEndedEventArgs : GameEventArgs
{
    public required Dictionary<string, int> RoundScores { get; init; }
}

public class GameEndedEventArgs : GameEventArgs
{
    public required string WinnerName { get; init; }
    public required string WinnerId { get; init; }
    public required Dictionary<string, int> FinalScores { get; init; }
}

public class GameErrorEventArgs : GameEventArgs
{
    public required string Message { get; init; }
}
