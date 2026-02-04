namespace BeziqueCore;

public static class HigherCardFinder
{
    public static bool HasHigherCard(List<Card> hand, Suit suit, Card currentWinner)
    {
        if (currentWinner.IsJoker) return hand.Exists(c => c.Suit == suit && !c.IsJoker);
        return hand.Exists(c => c.Suit == suit && c.Rank > currentWinner.Rank);
    }
}
