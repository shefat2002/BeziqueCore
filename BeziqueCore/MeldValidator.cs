namespace BeziqueCore;

public record MeldOpportunity(MeldType Type, Card[] Cards, int Points);

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

        int additionalPoints = 0;

        if (meldType == MeldType.TrumpRun && player.MeldHistory.ContainsKey(MeldType.TrumpMarriage))
        {
            additionalPoints = GetPoints(MeldType.TrumpRun) - GetPoints(MeldType.TrumpMarriage);
        }

        for (int i = 0; i < cards.Length; i++)
        {
            if (!PlayCardValidator.TryRemoveCard(player.Hand, cards[i]))
            {
                return false;
            }

            player.TableCards.Add(cards[i]);
        }

        player.RoundScore += GetPoints(meldType) + additionalPoints;

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

        int rankCount = 0;
        int jokerCount = 0;

        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i].IsJoker)
            {
                jokerCount++;
            }
            else if (cards[i].Rank == rank)
            {
                rankCount++;
            }
        }

        return rankCount + jokerCount == 4 && jokerCount <= 1;
    }

    public static MeldOpportunity? FindBestMeld(Player player, Suit trump)
    {
        var allCards = player.Hand.Concat(player.TableCards).ToList();
        var opportunities = new List<MeldOpportunity>();

        var trumpRunCards = FindTrumpRun(allCards, trump);
        if (trumpRunCards != null && ValidateMeld(MeldType.TrumpRun, trumpRunCards, trump) && CanUseCardsForMeld(player, trumpRunCards, MeldType.TrumpRun))
        {
            int points = GetPoints(MeldType.TrumpRun);
            if (player.MeldHistory.ContainsKey(MeldType.TrumpMarriage))
            {
                points += GetPoints(MeldType.TrumpRun) - GetPoints(MeldType.TrumpMarriage);
            }
            opportunities.Add(new MeldOpportunity(MeldType.TrumpRun, trumpRunCards, points));
        }

        var doubleBeziqueCards = FindDoubleBezique(allCards);
        if (doubleBeziqueCards != null && ValidateMeld(MeldType.DoubleBezique, doubleBeziqueCards, trump) && CanUseCardsForMeld(player, doubleBeziqueCards, MeldType.DoubleBezique))
        {
            opportunities.Add(new MeldOpportunity(MeldType.DoubleBezique, doubleBeziqueCards, GetPoints(MeldType.DoubleBezique)));
        }

        var fourAcesCards = FindFourOfAKind(allCards, Rank.Ace);
        if (fourAcesCards != null && ValidateMeld(MeldType.FourAces, fourAcesCards, trump) && CanUseCardsForMeld(player, fourAcesCards, MeldType.FourAces))
        {
            opportunities.Add(new MeldOpportunity(MeldType.FourAces, fourAcesCards, GetPoints(MeldType.FourAces)));
        }

        var fourKingsCards = FindFourOfAKind(allCards, Rank.King);
        if (fourKingsCards != null && ValidateMeld(MeldType.FourKings, fourKingsCards, trump) && CanUseCardsForMeld(player, fourKingsCards, MeldType.FourKings))
        {
            opportunities.Add(new MeldOpportunity(MeldType.FourKings, fourKingsCards, GetPoints(MeldType.FourKings)));
        }

        var fourQueensCards = FindFourOfAKind(allCards, Rank.Queen);
        if (fourQueensCards != null && ValidateMeld(MeldType.FourQueens, fourQueensCards, trump) && CanUseCardsForMeld(player, fourQueensCards, MeldType.FourQueens))
        {
            opportunities.Add(new MeldOpportunity(MeldType.FourQueens, fourQueensCards, GetPoints(MeldType.FourQueens)));
        }

        var fourJacksCards = FindFourOfAKind(allCards, Rank.Jack);
        if (fourJacksCards != null && ValidateMeld(MeldType.FourJacks, fourJacksCards, trump) && CanUseCardsForMeld(player, fourJacksCards, MeldType.FourJacks))
        {
            opportunities.Add(new MeldOpportunity(MeldType.FourJacks, fourJacksCards, GetPoints(MeldType.FourJacks)));
        }

        var trumpMarriageCards = FindMarriage(allCards, trump, true);
        if (trumpMarriageCards != null && ValidateMeld(MeldType.TrumpMarriage, trumpMarriageCards, trump) && CanUseCardsForMeld(player, trumpMarriageCards, MeldType.TrumpMarriage))
        {
            opportunities.Add(new MeldOpportunity(MeldType.TrumpMarriage, trumpMarriageCards, GetPoints(MeldType.TrumpMarriage)));
        }

        for (Suit s = Suit.Diamonds; s <= Suit.Spades; s++)
        {
            if (s == trump) continue;
            var marriageCards = FindMarriage(allCards, s, false);
            if (marriageCards != null && ValidateMeld(MeldType.NonTrumpMarriage, marriageCards, trump) && CanUseCardsForMeld(player, marriageCards, MeldType.NonTrumpMarriage))
            {
                opportunities.Add(new MeldOpportunity(MeldType.NonTrumpMarriage, marriageCards, GetPoints(MeldType.NonTrumpMarriage)));
            }
        }

        var beziqueCards = FindBezique(allCards);
        if (beziqueCards != null && ValidateMeld(MeldType.Bezique, beziqueCards, trump) && CanUseCardsForMeld(player, beziqueCards, MeldType.Bezique))
        {
            opportunities.Add(new MeldOpportunity(MeldType.Bezique, beziqueCards, GetPoints(MeldType.Bezique)));
        }

        var trumpSevenCards = FindTrumpSeven(allCards, trump);
        if (trumpSevenCards != null && ValidateMeld(MeldType.TrumpSeven, trumpSevenCards, trump) && CanUseCardsForMeld(player, trumpSevenCards, MeldType.TrumpSeven))
        {
            opportunities.Add(new MeldOpportunity(MeldType.TrumpSeven, trumpSevenCards, GetPoints(MeldType.TrumpSeven)));
        }

        if (opportunities.Count == 0) return null;

        var best = opportunities.OrderByDescending(o => o.Points).First();
        return best;
    }

    private static Card[]? FindTrumpRun(List<Card> cards, Suit trump)
    {
        var ace = cards.FirstOrDefault(c => c.Rank == Rank.Ace && c.Suit == trump);
        var ten = cards.FirstOrDefault(c => c.Rank == Rank.Ten && c.Suit == trump);
        var king = cards.FirstOrDefault(c => c.Rank == Rank.King && c.Suit == trump);
        var queen = cards.FirstOrDefault(c => c.Rank == Rank.Queen && c.Suit == trump);
        var jack = cards.FirstOrDefault(c => c.Rank == Rank.Jack && c.Suit == trump);

        if (ace == null || ten == null || king == null || queen == null || jack == null) return null;
        return new[] { ace, ten, king, queen, jack };
    }

    private static Card[]? FindDoubleBezique(List<Card> cards)
    {
        var queenSpades = cards.Where(c => c.Rank == Rank.Queen && c.Suit == Suit.Spades).Take(2).ToList();
        var jackDiamonds = cards.Where(c => c.Rank == Rank.Jack && c.Suit == Suit.Diamonds).Take(2).ToList();

        if (queenSpades.Count < 2 || jackDiamonds.Count < 2) return null;
        return queenSpades.Concat(jackDiamonds).ToArray();
    }

    private static Card[]? FindFourOfAKind(List<Card> cards, Rank rank)
    {
        var rankCards = cards.Where(c => c.Rank == rank).ToList();
        var jokers = cards.Where(c => c.IsJoker).ToList();

        int needed = 4;
        var result = new List<Card>();

        foreach (var card in rankCards)
        {
            if (result.Count < needed) result.Add(card);
        }

        foreach (var joker in jokers)
        {
            if (result.Count < needed) result.Add(joker);
        }

        if (result.Count == 4) return result.ToArray();
        return null;
    }

    private static Card[]? FindMarriage(List<Card> cards, Suit suit, bool isTrump)
    {
        var king = cards.FirstOrDefault(c => c.Rank == Rank.King && c.Suit == suit);
        var queen = cards.FirstOrDefault(c => c.Rank == Rank.Queen && c.Suit == suit);

        if (king == null || queen == null) return null;
        return new[] { king, queen };
    }

    private static Card[]? FindBezique(List<Card> cards)
    {
        var queenSpades = cards.FirstOrDefault(c => c.Rank == Rank.Queen && c.Suit == Suit.Spades);
        var jackDiamonds = cards.FirstOrDefault(c => c.Rank == Rank.Jack && c.Suit == Suit.Diamonds);

        if (queenSpades == null || jackDiamonds == null) return null;
        return new[] { queenSpades, jackDiamonds };
    }

    private static Card[]? FindTrumpSeven(List<Card> cards, Suit trump)
    {
        var seven = cards.FirstOrDefault(c => c.Rank == Rank.Seven && c.Suit == trump);
        if (seven == null) return null;
        return new[] { seven };
    }
}
