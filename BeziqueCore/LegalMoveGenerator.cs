namespace BeziqueCore;

public static class LegalMoveGenerator
{
    public static List<Card> GetLegalMoves(List<Card> hand, Suit leadSuit, Card? currentWinner, Suit trump)
    {
        if (!currentWinner.HasValue) return new List<Card>(hand);

        var legalMoves = new List<Card>();

        foreach (var card in hand)
        {
            if (LegalMoveValidator.IsLegalMove(hand, card, leadSuit, currentWinner, trump))
            {
                legalMoves.Add(card);
            }
        }

        return legalMoves;
    }
}
