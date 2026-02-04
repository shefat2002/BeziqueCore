namespace BeziqueCore;

public static class TrumpChecker
{
    public static bool HasTrumpSuit(List<Card> hand, Suit trump) =>
        hand.Exists(c => c.Suit == trump);

    public static List<Card> FindTrumpCards(List<Card> hand, Suit trump) =>
        hand.Where(c => c.Suit == trump).ToList();
}
