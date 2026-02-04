namespace BeziqueCore;

public static class LegalMoveValidator
{
    public static bool IsLegalMove(List<Card> hand, Card cardToPlay, Suit leadSuit, Card? currentWinner, Suit trump)
    {
        if (!currentWinner.HasValue) return true;

        bool hasLeadSuit = LeadSuitFinder.HasLeadSuit(hand, leadSuit);

        if (hasLeadSuit)
        {
            if (cardToPlay.Suit != leadSuit) return false;
            bool hasHigher = HigherCardFinder.HasHigherCard(hand, leadSuit, currentWinner.Value);
            if (hasHigher && cardToPlay.Rank <= currentWinner.Value.Rank) return false;
            return true;
        }

        bool hasTrump = TrumpChecker.HasTrumpSuit(hand, trump);

        if (hasTrump)
        {
            if (cardToPlay.Suit != trump) return false;
            if (currentWinner.Value.Suit == trump && currentWinner.Value.Rank > cardToPlay.Rank)
            {
                bool hasHigherTrump = HigherCardFinder.HasHigherCard(hand, trump, currentWinner.Value);
                if (hasHigherTrump) return false;
            }
            return true;
        }

        return true;
    }
}
