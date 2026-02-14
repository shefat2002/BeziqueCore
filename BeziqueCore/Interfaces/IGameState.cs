namespace BeziqueCore.Interfaces;

public enum GamePhase
{
    Dealing,
    TrumpFlip,
    Phase1_Playing,
    Phase2_Playing,
    RoundEnd,
    GameEnd
}

public interface IGameState
{
    GamePhase CurrentPhase { get; }
    IReadOnlyList<IPlayerHand> Players { get; }
    IGameCard? TrumpCard { get; }
    byte? TrumpSuit { get; }
    int DeckCount { get; }
    int CurrentPlayerIndex { get; }
    int DealerIndex { get; }
}
