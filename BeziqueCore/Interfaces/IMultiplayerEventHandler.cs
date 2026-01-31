using BeziqueCore.Models.Events;

namespace BeziqueCore.Interfaces;

public interface IMultiplayerEventHandler
{
    void OnCardPlayed(CardPlayedEvent gameEvent);
    void OnMeldDeclared(MeldDeclaredEvent gameEvent);
    void OnMeldSkipped(MeldSkippedEvent gameEvent);
    void OnTrickResolved(TrickResolvedEvent gameEvent);
    void OnSevenOfTrumpSwitched(SevenOfTrumpSwitchedEvent gameEvent);
    void OnCardsDrawn(CardsDrawnEvent gameEvent);
    void OnRoundEnded(RoundEndedEvent gameEvent);
    void OnGameEnded(GameEndedEvent gameEvent);
    void OnLastNineCardsStarted(LastNineCardsStartedEvent gameEvent);
    void OnTrumpDetermined(TrumpDeterminedEvent gameEvent);
    void OnPlayerTurnChanged(PlayerTurnChangedEvent gameEvent);
    void OnError(GameErrorEvent gameEvent);
}
