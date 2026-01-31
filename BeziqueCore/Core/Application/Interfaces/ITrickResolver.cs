using BeziqueCore.Core.Domain.Entities;

namespace BeziqueCore.Core.Application.Interfaces
{
    public interface ITrickResolver
    {
        Player DetermineTrickWinner(
            Dictionary<Player, Card> playedCards,
            Suit trumpSuit,
            Suit leadSuit);

        bool IsValidPlay(
            Card cardToPlay,
            List<Card> playerHand,
            Suit leadSuit,
            bool isLastNineCards);

        int CalculateTrickPoints(List<Card> cards);
        bool HasJoker(Dictionary<Player, Card> playedCards);
        Suit GetLeadSuit(Dictionary<Player, Card> playedCards);
    }
}
