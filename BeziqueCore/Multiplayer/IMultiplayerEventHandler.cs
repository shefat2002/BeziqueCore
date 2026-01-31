using BeziqueCore.Multiplayer.Models.Events;

namespace BeziqueCore.Multiplayer;

/// <summary>
/// Interface for handling multiplayer-specific game events.
/// Implementations can forward events to SignalR, Orleans, or other network systems.
/// </summary>
public interface IMultiplayerEventHandler
{
    /// <summary>
    /// Called when a card is played by a player.
    /// </summary>
    void OnCardPlayed(CardPlayedEvent gameEvent);

    /// <summary>
    /// Called when a meld is declared by a player.
    /// </summary>
    void OnMeldDeclared(MeldDeclaredEvent gameEvent);

    /// <summary>
    /// Called when a player skips meld declaration.
    /// </summary>
    void OnMeldSkipped(MeldSkippedEvent gameEvent);

    /// <summary>
    /// Called when a trick is resolved.
    /// </summary>
    void OnTrickResolved(TrickResolvedEvent gameEvent);

    /// <summary>
    /// Called when the seven of trump is switched.
    /// </summary>
    void OnSevenOfTrumpSwitched(SevenOfTrumpSwitchedEvent gameEvent);

    /// <summary>
    /// Called when cards are drawn.
    /// </summary>
    void OnCardsDrawn(CardsDrawnEvent gameEvent);

    /// <summary>
    /// Called when the round ends.
    /// </summary>
    void OnRoundEnded(RoundEndedEvent gameEvent);

    /// <summary>
    /// Called when the game ends.
    /// </summary>
    void OnGameEnded(GameEndedEvent gameEvent);

    /// <summary>
    /// Called when the last nine cards phase begins.
    /// </summary>
    void OnLastNineCardsStarted(LastNineCardsStartedEvent gameEvent);

    /// <summary>
    /// Called when the trump card is determined.
    /// </summary>
    void OnTrumpDetermined(TrumpDeterminedEvent gameEvent);

    /// <summary>
    /// Called when a player turn changes.
    /// </summary>
    void OnPlayerTurnChanged(PlayerTurnChangedEvent gameEvent);

    /// <summary>
    /// Called when an error occurs during game execution.
    /// </summary>
    void OnError(GameErrorEvent gameEvent);
}
