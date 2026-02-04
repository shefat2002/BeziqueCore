namespace BeziqueCore;

public static class PlayCardValidator
{
    public static bool CanPlayCard(Player player, Card card)
    {
        return ContainsCard(player.Hand, card) || ContainsCard(player.TableCards, card);
    }

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

    public static bool TryPlayCard(Player player, Card card, out Card playedCard)
    {
        if (TryRemoveCard(player.Hand, card))
        {
            playedCard = card;
            return true;
        }

        if (TryRemoveCard(player.TableCards, card))
        {
            playedCard = card;
            return true;
        }

        playedCard = default;
        return false;
    }
}
