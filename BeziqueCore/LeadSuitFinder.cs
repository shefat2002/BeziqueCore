namespace BeziqueCore;

public static class LeadSuitFinder
{
    public static bool HasLeadSuit(List<Card> hand, Suit leadSuit) =>
        hand.Exists(c => c.Suit == leadSuit);
}
