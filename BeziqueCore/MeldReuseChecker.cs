namespace BeziqueCore;

public static class MeldReuseChecker
{
    public static bool HasCardBeenUsedForMeld(Player player, Card card, MeldType meldType)
    {
        if (!player.MeldHistory.TryGetValue(meldType, out var usedCards))
        {
            return false;
        }

        for (int i = 0; i < usedCards.Count; i++)
        {
            if (usedCards[i].CardId == card.CardId && usedCards[i].DeckIndex == card.DeckIndex)
            {
                return true;
            }
        }

        return false;
    }

    public static bool CanUseCardsForMeld(Player player, Card[] cards, MeldType meldType)
    {
        for (int i = 0; i < cards.Length; i++)
        {
            if (HasCardBeenUsedForMeld(player, cards[i], meldType))
            {
                return false;
            }
        }

        return true;
    }

    public static bool AnyCardsAlreadyUsed(Player player, Card[] cards, MeldType meldType)
    {
        for (int i = 0; i < cards.Length; i++)
        {
            if (HasCardBeenUsedForMeld(player, cards[i], meldType))
            {
                return true;
            }
        }

        return false;
    }
}
