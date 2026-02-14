namespace BeziqueCore.Interfaces;

public interface IPlayerHand
{
    int PlayerIndex { get; }
    IReadOnlyList<IGameCard> Cards { get; }
    int Score { get; }
    bool IsCurrentPlayer { get; }
}
