namespace BeziqueCore;

public static class MeldExecutor
{
    public static bool TryExecuteMeld(Player player, Card[] cards, MeldType meldType, Suit trump)
    {
        if (!MeldRuleValidator.ValidateMeld(meldType, cards, trump)) return false;
        if (!MeldReuseChecker.CanUseCardsForMeld(player, cards, meldType)) return false;

        for (int i = 0; i < cards.Length; i++)
        {
            if (!PlayCardValidator.TryRemoveCard(player.Hand, cards[i]))
            {
                return false;
            }

            player.TableCards.Add(cards[i]);
        }

        player.RoundScore += MeldDefinitions.GetPoints(meldType);

        if (!player.MeldHistory.ContainsKey(meldType))
        {
            player.MeldHistory[meldType] = new List<Card>();
        }

        player.MeldHistory[meldType].AddRange(cards);

        return true;
    }

    public static bool TryExecuteMeld(Player player, List<Card> cards, MeldType meldType, Suit trump)
    {
        if (!MeldRuleValidator.ValidateMeld(meldType, cards.ToArray(), trump)) return false;
        if (!MeldReuseChecker.CanUseCardsForMeld(player, cards.ToArray(), meldType)) return false;

        for (int i = 0; i < cards.Count; i++)
        {
            if (!PlayCardValidator.TryRemoveCard(player.Hand, cards[i]))
            {
                return false;
            }

            player.TableCards.Add(cards[i]);
        }

        player.RoundScore += MeldDefinitions.GetPoints(meldType);

        if (!player.MeldHistory.ContainsKey(meldType))
        {
            player.MeldHistory[meldType] = new List<Card>();
        }

        player.MeldHistory[meldType].AddRange(cards);

        return true;
    }
}
