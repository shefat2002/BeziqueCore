namespace BeziqueCore;

public static class MeldRuleValidator
{
    public static bool ValidateMeld(MeldType meldType, Card[] cards, Suit trump)
    {
        if (cards.Length != MeldDefinitions.GetRequiredCardCount(meldType)) return false;

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
