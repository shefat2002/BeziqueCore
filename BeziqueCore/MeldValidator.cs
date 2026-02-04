namespace BeziqueCore;

public static class MeldValidator
{
    public static int GetPoints(MeldType meldType)
    {
        return meldType switch
        {
            MeldType.TrumpSeven => 10,
            MeldType.TrumpMarriage => 40,
            MeldType.NonTrumpMarriage => 20,
            MeldType.Bezique => 40,
            MeldType.DoubleBezique => 500,
            MeldType.TrumpRun => 250,
            MeldType.FourAces => 100,
            MeldType.FourKings => 80,
            MeldType.FourQueens => 60,
            MeldType.FourJacks => 40,
            _ => 0
        };
    }

    public static int GetRequiredCardCount(MeldType meldType)
    {
        return meldType switch
        {
            MeldType.TrumpSeven => 1,
            MeldType.TrumpMarriage => 2,
            MeldType.NonTrumpMarriage => 2,
            MeldType.Bezique => 2,
            MeldType.DoubleBezique => 4,
            MeldType.TrumpRun => 5,
            MeldType.FourAces => 4,
            MeldType.FourKings => 4,
            MeldType.FourQueens => 4,
            MeldType.FourJacks => 4,
            _ => 0
        };
    }

    public static bool ValidateMeld(MeldType meldType, Card[] cards, Suit trump)
    {
        if (cards.Length != GetRequiredCardCount(meldType)) return false;

        return meldType switch
        {
            MeldType.TrumpSeven => ValidateTrumpSeven(cards, trump),
            MeldType.TrumpMarriage => ValidateTrumpMarriage(cards, trump),
            MeldType.NonTrumpMarriage => ValidateNonTrumpMarriage(cards),
            MeldType.Bezique => ValidateBezique(cards),
            MeldType.DoubleBezique => ValidateDoubleBezique(cards),
            MeldType.TrumpRun => ValidateTrumpRun(cards, trump),
            MeldType.FourAces => ValidateFourOfAKind(cards, Rank.Ace),
            MeldType.FourKings => ValidateFourOfAKind(cards, Rank.King),
            MeldType.FourQueens => ValidateFourOfAKind(cards, Rank.Queen),
            MeldType.FourJacks => ValidateFourOfAKind(cards, Rank.Jack),
            _ => false
        };
    }

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

    public static bool TryExecuteMeld(Player player, Card[] cards, MeldType meldType, Suit trump)
    {
        if (!ValidateMeld(meldType, cards, trump)) return false;
        if (!CanUseCardsForMeld(player, cards, meldType)) return false;

        for (int i = 0; i < cards.Length; i++)
        {
            if (!PlayCardValidator.TryRemoveCard(player.Hand, cards[i]))
            {
                return false;
            }

            player.TableCards.Add(cards[i]);
        }

        player.RoundScore += GetPoints(meldType);

        if (!player.MeldHistory.ContainsKey(meldType))
        {
            player.MeldHistory[meldType] = new List<Card>();
        }

        player.MeldHistory[meldType].AddRange(cards);

        return true;
    }

    public static bool TryExecuteMeld(Player player, List<Card> cards, MeldType meldType, Suit trump)
    {
        if (!ValidateMeld(meldType, cards.ToArray(), trump)) return false;
        if (!CanUseCardsForMeld(player, cards.ToArray(), meldType)) return false;

        for (int i = 0; i < cards.Count; i++)
        {
            if (!PlayCardValidator.TryRemoveCard(player.Hand, cards[i]))
            {
                return false;
            }

            player.TableCards.Add(cards[i]);
        }

        player.RoundScore += GetPoints(meldType);

        if (!player.MeldHistory.ContainsKey(meldType))
        {
            player.MeldHistory[meldType] = new List<Card>();
        }

        player.MeldHistory[meldType].AddRange(cards);

        return true;
    }

    private static bool ValidateTrumpSeven(Card[] cards, Suit trump)
    {
        return cards.Length == 1 && cards[0].Rank == Rank.Seven && cards[0].Suit == trump;
    }

    private static bool ValidateTrumpMarriage(Card[] cards, Suit trump)
    {
        if (cards.Length != 2) return false;

        bool hasKing = cards.Any(c => c.Rank == Rank.King && c.Suit == trump);
        bool hasQueen = cards.Any(c => c.Rank == Rank.Queen && c.Suit == trump);

        return hasKing && hasQueen;
    }

    private static bool ValidateNonTrumpMarriage(Card[] cards)
    {
        if (cards.Length != 2) return false;

        var king = cards.FirstOrDefault(c => c.Rank == Rank.King);
        var queen = cards.FirstOrDefault(c => c.Rank == Rank.Queen);

        if (king == default || queen == default) return false;

        return king.Suit == queen.Suit && king.Suit != Suit.None;
    }

    private static bool ValidateBezique(Card[] cards)
    {
        if (cards.Length != 2) return false;

        bool hasQueenSpades = cards.Any(c => c.Rank == Rank.Queen && c.Suit == Suit.Spades);
        bool hasJackDiamonds = cards.Any(c => c.Rank == Rank.Jack && c.Suit == Suit.Diamonds);

        return hasQueenSpades && hasJackDiamonds;
    }

    private static bool ValidateDoubleBezique(Card[] cards)
    {
        if (cards.Length != 4) return false;

        int queenSpadesCount = cards.Count(c => c.Rank == Rank.Queen && c.Suit == Suit.Spades);
        int jackDiamondsCount = cards.Count(c => c.Rank == Rank.Jack && c.Suit == Suit.Diamonds);

        return queenSpadesCount == 2 && jackDiamondsCount == 2;
    }

    private static bool ValidateTrumpRun(Card[] cards, Suit trump)
    {
        if (cards.Length != 5) return false;

        bool hasAce = cards.Any(c => c.Rank == Rank.Ace && c.Suit == trump);
        bool hasTen = cards.Any(c => c.Rank == Rank.Ten && c.Suit == trump);
        bool hasKing = cards.Any(c => c.Rank == Rank.King && c.Suit == trump);
        bool hasQueen = cards.Any(c => c.Rank == Rank.Queen && c.Suit == trump);
        bool hasJack = cards.Any(c => c.Rank == Rank.Jack && c.Suit == trump);

        return hasAce && hasTen && hasKing && hasQueen && hasJack;
    }

    private static bool ValidateFourOfAKind(Card[] cards, Rank rank)
    {
        if (cards.Length != 4) return false;

        int count = 0;
        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i].Rank == rank) count++;
        }

        return count == 4;
    }
}
