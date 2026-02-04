namespace BeziqueCore;

public static class Phase2MoveValidator
{
    public static bool HasLeadSuit(List<Card> hand, Suit leadSuit) =>
        hand.Exists(c => c.Suit == leadSuit);

    public static bool HasHigherCard(List<Card> hand, Suit suit, Card currentWinner)
    {
        if (currentWinner.IsJoker) return hand.Exists(c => c.Suit == suit && !c.IsJoker);
        return hand.Exists(c => c.Suit == suit && c.Rank > currentWinner.Rank);
    }

    public static bool HasTrumpSuit(List<Card> hand, Suit trump) =>
        hand.Exists(c => c.Suit == trump);

    public static List<Card> FindTrumpCards(List<Card> hand, Suit trump) =>
        hand.Where(c => c.Suit == trump).ToList();

    public static bool IsLegalMove(List<Card> hand, Card cardToPlay, Suit leadSuit, Card? currentWinner, Suit trump)
    {
        if (!currentWinner.HasValue) return true;

        bool hasLeadSuit = HasLeadSuit(hand, leadSuit);

        if (hasLeadSuit)
        {
            if (cardToPlay.Suit != leadSuit) return false;
            bool hasHigher = HasHigherCard(hand, leadSuit, currentWinner.Value);
            if (hasHigher && cardToPlay.Rank <= currentWinner.Value.Rank) return false;
            return true;
        }

        bool hasTrump = HasTrumpSuit(hand, trump);

        if (hasTrump)
        {
            if (cardToPlay.Suit != trump) return false;
            if (currentWinner.Value.Suit == trump && currentWinner.Value.Rank > cardToPlay.Rank)
            {
                bool hasHigherTrump = HasHigherCard(hand, trump, currentWinner.Value);
                if (hasHigherTrump) return false;
            }
            return true;
        }

        return true;
    }

    public static List<Card> GetLegalMoves(List<Card> hand, Suit leadSuit, Card? currentWinner, Suit trump)
    {
        if (!currentWinner.HasValue) return new List<Card>(hand);

        var legalMoves = new List<Card>();

        foreach (var card in hand)
        {
            if (IsLegalMove(hand, card, leadSuit, currentWinner, trump))
            {
                legalMoves.Add(card);
            }
        }

        return legalMoves;
    }
}
