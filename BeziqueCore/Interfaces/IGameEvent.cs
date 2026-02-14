namespace BeziqueCore.Interfaces;

public interface IGameEvent
{
    event Action<int, IReadOnlyList<IGameCard>> CardsDealt;
    event Action<IGameCard> TrumpCardFlipped;
    event Action<int> DealerBonusPoints;
    event Action<int, IGameCard> CardDrawn;
    event Action<GamePhase> PhaseChanged;
    event Action<int> TrickComplete;
    event Action<int, int> MeldPointsAwarded;
    event Action<int> RoundEnd;
    event Action<int> GameEnd;
}
