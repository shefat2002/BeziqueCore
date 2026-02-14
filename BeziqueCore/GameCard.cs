using BeziqueCore.Interfaces;

namespace BeziqueCore;

public class GameCard : IGameCard
{
    public byte CardValue { get; }
    public byte DeckIndex { get; }
    public byte CardId { get; }
    public bool IsJoker { get; }

    public GameCard(byte cardId)
    {
        CardId = cardId;
        CardValue = CardHelper.GetCardValue(cardId);
        DeckIndex = CardHelper.GetDeckIndex(cardId);
        IsJoker = CardHelper.IsJoker(cardId);
    }
}