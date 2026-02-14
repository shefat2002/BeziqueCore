namespace BeziqueCore.Interfaces;

public interface IBeziqueGame : IGameState, IGameEvent
{
    void Initialize(int playerCount);
    void StartDealing();
    bool DealNextSet();
    void CompleteDealing();
    void PlayCard(int playerIndex, byte cardId);
    void DrawCard(int playerIndex);
    void CreateMeld(int playerIndex, MeldType meldType, byte[] cardIds);
    bool IsPlayerTurn(int playerIndex);
    IReadOnlyList<byte> GetPlayableCards(int playerIndex);
    void StartNewRound();
}

public enum MeldType
{
    Bezique,
    DoubleBezique,
    FourJacks,
    FourQueens,
    FourKings,
    FourAces,
    CommonMarriage,
    TrumpMarriage,
    TrumpRun,
}
