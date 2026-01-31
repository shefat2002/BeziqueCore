namespace BeziqueCore.Models.Events;

public record GameEvent
{
    public string EventType { get; init; } = nameof(GameEvent);
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

public record CardPlayedEvent : GameEvent
{
    public CardPlayedEvent() => EventType = nameof(CardPlayedEvent);
    public required string PlayerUserId { get; init; }
    public required CardDto Card { get; init; }
    public required int CardIndex { get; init; }
}

public record MeldDeclaredEvent : GameEvent
{
    public MeldDeclaredEvent() => EventType = nameof(MeldDeclaredEvent);
    public required string PlayerUserId { get; init; }
    public required MeldDto Meld { get; init; }
    public required int[] CardIndices { get; init; }
    public required int Points { get; init; }
}

public record MeldSkippedEvent : GameEvent
{
    public MeldSkippedEvent() => EventType = nameof(MeldSkippedEvent);
    public required string PlayerUserId { get; init; }
}

public record TrickResolvedEvent : GameEvent
{
    public TrickResolvedEvent() => EventType = nameof(TrickResolvedEvent);
    public required string WinnerUserId { get; init; }
    public required Dictionary<string, CardDto> PlayedCards { get; init; }
    public required int Points { get; init; }
}

public record SevenOfTrumpSwitchedEvent : GameEvent
{
    public SevenOfTrumpSwitchedEvent() => EventType = nameof(SevenOfTrumpSwitchedEvent);
    public required string PlayerUserId { get; init; }
}

public record CardsDrawnEvent : GameEvent
{
    public CardsDrawnEvent() => EventType = nameof(CardsDrawnEvent);
    public required string PlayerUserId { get; init; }
    public required int CardCount { get; init; }
    public required bool TookTrumpCard { get; init; }
}

public record RoundEndedEvent : GameEvent
{
    public RoundEndedEvent() => EventType = nameof(RoundEndedEvent);
    public required Dictionary<string, int> RoundScores { get; init; }
    public Dictionary<string, int> AcesAndTensBonus { get; init; } = new();
}

public record GameEndedEvent : GameEvent
{
    public GameEndedEvent() => EventType = nameof(GameEndedEvent);
    public required string WinnerUserId { get; init; }
    public required Dictionary<string, int> FinalScores { get; init; }
}

public record LastNineCardsStartedEvent : GameEvent
{
    public LastNineCardsStartedEvent() => EventType = nameof(LastNineCardsStartedEvent);
}

public record TrumpDeterminedEvent : GameEvent
{
    public TrumpDeterminedEvent() => EventType = nameof(TrumpDeterminedEvent);
    public required string TrumpSuit { get; init; }
    public required CardDto TrumpCard { get; init; }
}

public record PlayerTurnChangedEvent : GameEvent
{
    public PlayerTurnChangedEvent() => EventType = nameof(PlayerTurnChangedEvent);
    public required string PlayerUserId { get; init; }
    public required bool CanDeclareMeld { get; init; }
}

public record GameErrorEvent : GameEvent
{
    public GameErrorEvent() => EventType = nameof(GameErrorEvent);
    public required string ErrorMessage { get; init; }
    public string? ErrorCode { get; init; }
    public string? PlayerUserId { get; init; }
}
