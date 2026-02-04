namespace BeziqueCore;

public static class CardCollectionHelper
{
    public static List<Card> CombineHandAndTable(Player player)
    {
        var combined = new List<Card>(player.Hand.Count + player.TableCards.Count);
        combined.AddRange(player.Hand);
        combined.AddRange(player.TableCards);
        return combined;
    }

    public static bool TryFindCard(List<Card> cards, byte cardId, out Card foundCard)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i].CardId == cardId)
            {
                foundCard = cards[i];
                return true;
            }
        }

        foundCard = default;
        return false;
    }

    public static bool TryFindCards(List<Card> cards, byte[] cardIds, out List<Card> foundCards)
    {
        foundCards = new List<Card>(cardIds.Length);

        for (int j = 0; j < cardIds.Length; j++)
        {
            bool found = false;
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].CardId == cardIds[j])
                {
                    foundCards.Add(cards[i]);
                    found = true;
                    break;
                }
            }

            if (!found) return false;
        }

        return true;
    }

    public static List<Card> FindCardsBySuit(List<Card> cards, Suit suit)
    {
        var result = new List<Card>();

        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i].Suit == suit)
            {
                result.Add(cards[i]);
            }
        }

        return result;
    }

    public static List<Card> FindCardsByRank(List<Card> cards, Rank rank)
    {
        var result = new List<Card>();

        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i].Rank == rank)
            {
                result.Add(cards[i]);
            }
        }

        return result;
    }

    public static bool HasCard(List<Card> cards, byte cardId)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i].CardId == cardId) return true;
        }
        return false;
    }
}
