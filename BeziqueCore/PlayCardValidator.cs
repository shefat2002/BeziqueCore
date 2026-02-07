namespace BeziqueCore;

public static class PlayCardValidator
{
    public static bool ContainsCard(List<Card> cards, Card card)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] == card) return true;
        }
        return false;
    }

    public static bool TryRemoveCard(List<Card> cards, Card card)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] == card)
            {
                cards.RemoveAt(i);
                return true;
            }
        }
        return false;
    }
}
