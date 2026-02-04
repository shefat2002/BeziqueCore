namespace BeziqueCore;

public static class MeldStateHandler
{
    public static bool DeclareMeld(Player player, Card[] cards, MeldType meldType, Suit trump)
    {
        return MeldValidator.TryExecuteMeld(player, cards, meldType, trump);
    }

    public static bool DeclareMeld(Player player, List<Card> cards, MeldType meldType, Suit trump)
    {
        return MeldValidator.TryExecuteMeld(player, cards, meldType, trump);
    }
}
