namespace BeziqueCore.Multiplayer.Models.Events;

/// <summary>
/// Base class for all multiplayer game events.
/// </summary>
public record GameEvent
{
    /// <summary>
    /// The type of game event.
    /// </summary>
    public string EventType { get; init; } = nameof(GameEvent);

    /// <summary>
    /// Timestamp when the event occurred.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Event fired when a player plays a card.
/// </summary>
public record CardPlayedEvent : GameEvent
{
    public CardPlayedEvent()
    {
        EventType = nameof(CardPlayedEvent);
    }

    /// <summary>
    /// The user ID of the player who played the card.
    /// </summary>
    public required string PlayerUserId { get; init; }

    /// <summary>
    /// The card that was played.
    /// </summary>
    public required CardDto Card { get; init; }

    /// <summary>
    /// The index of the card in the player's hand.
    /// </summary>
    public required int CardIndex { get; init; }
}

/// <summary>
/// Event fired when a player declares a meld.
/// </summary>
public record MeldDeclaredEvent : GameEvent
{
    public MeldDeclaredEvent()
    {
        EventType = nameof(MeldDeclaredEvent);
    }

    /// <summary>
    /// The user ID of the player who declared the meld.
    /// </summary>
    public required string PlayerUserId { get; init; }

    /// <summary>
    /// The meld that was declared.
    /// </summary>
    public required MeldDto Meld { get; init; }

    /// <summary>
    /// The indices of the cards in the player's hand.
    /// </summary>
    public required int[] CardIndices { get; init; }

    /// <summary>
    /// The points awarded for the meld.
    /// </summary>
    public required int Points { get; init; }
}

/// <summary>
/// Event fired when a player skips meld declaration.
/// </summary>
public record MeldSkippedEvent : GameEvent
{
    public MeldSkippedEvent()
    {
        EventType = nameof(MeldSkippedEvent);
    }

    /// <summary>
    /// The user ID of the player who skipped meld.
    /// </summary>
    public required string PlayerUserId { get; init; }
}

/// <summary>
/// Event fired when a trick is resolved.
/// </summary>
public record TrickResolvedEvent : GameEvent
{
    public TrickResolvedEvent()
    {
        EventType = nameof(TrickResolvedEvent);
    }

    /// <summary>
    /// The user ID of the player who won the trick.
    /// </summary>
    public required string WinnerUserId { get; init; }

    /// <summary>
    /// The cards that were in the trick.
    /// </summary>
    public required Dictionary<string, CardDto> PlayedCards { get; init; }

    /// <summary>
    /// The points awarded for winning the trick.
    /// </summary>
    public required int Points { get; init; }
}

/// <summary>
/// Event fired when the seven of trump is switched.
/// </summary>
public record SevenOfTrumpSwitchedEvent : GameEvent
{
    public SevenOfTrumpSwitchedEvent()
    {
        EventType = nameof(SevenOfTrumpSwitchedEvent);
    }

    /// <summary>
    /// The user ID of the player who switched the seven of trump.
    /// </summary>
    public required string PlayerUserId { get; init; }
}

/// <summary>
/// Event fired when cards are drawn.
/// </summary>
public record CardsDrawnEvent : GameEvent
{
    public CardsDrawnEvent()
    {
        EventType = nameof(CardsDrawnEvent);
    }

    /// <summary>
    /// The user ID of the player who drew cards.
    /// </summary>
    public required string PlayerUserId { get; init; }

    /// <summary>
    /// The number of cards drawn.
    /// </summary>
    public required int CardCount { get; init; }

    /// <summary>
    /// Whether the trump card was taken.
    /// </summary>
    public required bool TookTrumpCard { get; init; }
}

/// <summary>
/// Event fired when a round ends.
/// </summary>
public record RoundEndedEvent : GameEvent
{
    public RoundEndedEvent()
    {
        EventType = nameof(RoundEndedEvent);
    }

    /// <summary>
    /// The round scores for all players.
    /// </summary>
    public required Dictionary<string, int> RoundScores { get; init; }

    /// <summary>
    /// The aces and tens bonus points for each player.
    /// </summary>
    public Dictionary<string, int> AcesAndTensBonus { get; init; } = new();
}

/// <summary>
/// Event fired when the game ends.
/// </summary>
public record GameEndedEvent : GameEvent
{
    public GameEndedEvent()
    {
        EventType = nameof(GameEndedEvent);
    }

    /// <summary>
    /// The user ID of the winner.
    /// </summary>
    public required string WinnerUserId { get; init; }

    /// <summary>
    /// The final scores for all players.
    /// </summary>
    public required Dictionary<string, int> FinalScores { get; init; }
}

/// <summary>
/// Event fired when the last nine cards phase begins.
/// </summary>
public record LastNineCardsStartedEvent : GameEvent
{
    public LastNineCardsStartedEvent()
    {
        EventType = nameof(LastNineCardsStartedEvent);
    }
}

/// <summary>
/// Event fired when the trump card is determined.
/// </summary>
public record TrumpDeterminedEvent : GameEvent
{
    public TrumpDeterminedEvent()
    {
        EventType = nameof(TrumpDeterminedEvent);
    }

    /// <summary>
    /// The trump suit.
    /// </summary>
    public required string TrumpSuit { get; init; }

    /// <summary>
    /// The trump card.
    /// </summary>
    public required CardDto TrumpCard { get; init; }
}

/// <summary>
/// Event fired when the player turn changes.
/// </summary>
public record PlayerTurnChangedEvent : GameEvent
{
    public PlayerTurnChangedEvent()
    {
        EventType = nameof(PlayerTurnChangedEvent);
    }

    /// <summary>
    /// The user ID of the player whose turn it is now.
    /// </summary>
    public required string PlayerUserId { get; init; }

    /// <summary>
    /// Whether meld declaration is allowed in this turn.
    /// </summary>
    public required bool CanDeclareMeld { get; init; }
}

/// <summary>
/// Event fired when an error occurs during game execution.
/// </summary>
public record GameErrorEvent : GameEvent
{
    public GameErrorEvent()
    {
        EventType = nameof(GameErrorEvent);
    }

    /// <summary>
    /// The error message.
    /// </summary>
    public required string ErrorMessage { get; init; }

    /// <summary>
    /// The error code if applicable.
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// The user ID associated with the error (if applicable).
    /// </summary>
    public string? PlayerUserId { get; init; }
}
