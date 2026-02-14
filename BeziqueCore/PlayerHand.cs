using BeziqueCore.Interfaces;

namespace BeziqueCore;

public class PlayerHand : IPlayerHand
{
    private readonly List<byte> _cardIds;
    private readonly List<IGameCard> _cards;

    public PlayerHand(int playerIndex, List<byte> cardIds, int score)
    {
        PlayerIndex = playerIndex;
        _cardIds = cardIds;
        Score = score;
        _cards = cardIds.Select(id => new GameCard(id)).ToList<IGameCard>();
    }

    public int PlayerIndex { get; }
    public IReadOnlyList<IGameCard> Cards => _cards.AsReadOnly();
    public int Score { get; }
    public bool IsCurrentPlayer { get; set; }
}