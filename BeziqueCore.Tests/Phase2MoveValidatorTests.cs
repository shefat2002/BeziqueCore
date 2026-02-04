using BeziqueCore;

public class Phase2MoveValidatorTests
{
    #region LeadSuitFinder Tests

    [Fact]
    public void HasLeadSuit_ReturnsTrue_WhenHandContainsLeadSuit()
    {
        var hand = new List<Card> { new Card((byte)0, 0), new Card((byte)4, 0) };
        Assert.True(LeadSuitFinder.HasLeadSuit(hand, Suit.Diamonds));
    }

    [Fact]
    public void HasLeadSuit_ReturnsFalse_WhenHandDoesNotContainLeadSuit()
    {
        var hand = new List<Card> { new Card((byte)1, 0), new Card((byte)5, 0) };
        Assert.False(LeadSuitFinder.HasLeadSuit(hand, Suit.Diamonds));
    }

    #endregion

    #region HigherCardFinder Tests

    [Fact]
    public void HasHigherCard_ReturnsTrue_WhenHandHasHigherRankOfSameSuit()
    {
        var hand = new List<Card> { new Card((byte)28, 0) };
        var winner = new Card((byte)24, 0);
        Assert.True(HigherCardFinder.HasHigherCard(hand, Suit.Diamonds, winner));
    }

    [Fact]
    public void HasHigherCard_ReturnsFalse_WhenHandHasNoHigherRankOfSameSuit()
    {
        var hand = new List<Card> { new Card((byte)8, 0) };
        var winner = new Card((byte)24, 0);
        Assert.False(HigherCardFinder.HasHigherCard(hand, Suit.Diamonds, winner));
    }

    [Fact]
    public void HasHigherCard_ReturnsTrue_WhenCurrentWinnerIsJoker()
    {
        var hand = new List<Card> { new Card((byte)0, 0) };
        var joker = new Card((byte)32, 0);
        Assert.True(HigherCardFinder.HasHigherCard(hand, Suit.Diamonds, joker));
    }

    [Fact]
    public void HasHigherCard_ReturnsFalse_WhenHandIsEmpty()
    {
        var hand = new List<Card>();
        var winner = new Card((byte)0, 0);
        Assert.False(HigherCardFinder.HasHigherCard(hand, Suit.Diamonds, winner));
    }

    #endregion

    #region TrumpChecker Tests

    [Fact]
    public void HasTrumpSuit_ReturnsTrue_WhenHandContainsTrump()
    {
        var hand = new List<Card> { new Card((byte)0, 0), new Card((byte)1, 0) };
        Assert.True(TrumpChecker.HasTrumpSuit(hand, Suit.Diamonds));
    }

    [Fact]
    public void HasTrumpSuit_ReturnsFalse_WhenHandDoesNotContainTrump()
    {
        var hand = new List<Card> { new Card((byte)1, 0), new Card((byte)6, 0) };
        Assert.False(TrumpChecker.HasTrumpSuit(hand, Suit.Diamonds));
    }

    [Fact]
    public void FindTrumpCards_ReturnsOnlyTrumpCards()
    {
        var hand = new List<Card>
        {
            new Card((byte)0, 0),
            new Card((byte)4, 0),
            new Card((byte)1, 0)
        };
        var trumps = TrumpChecker.FindTrumpCards(hand, Suit.Diamonds);
        Assert.Equal(2, trumps.Count);
        Assert.All(trumps, c => Assert.Equal(Suit.Diamonds, c.Suit));
    }

    #endregion

    #region LegalMoveValidator Tests

    [Fact]
    public void IsLegalMove_NoCurrentWinner_AllCardsLegal()
    {
        var hand = new List<Card> { new Card((byte)0, 0), new Card((byte)1, 0) };
        Assert.True(LegalMoveValidator.IsLegalMove(hand, hand[0], Suit.Diamonds, null, Suit.Spades));
    }

    [Fact]
    public void IsLegalMove_HasLeadSuit_MustPlayLeadSuit()
    {
        var hand = new List<Card>
        {
            new Card((byte)0, 0),
            new Card((byte)1, 0)
        };
        var winner = new Card((byte)8, 0);
        Assert.True(LegalMoveValidator.IsLegalMove(hand, hand[0], Suit.Diamonds, winner, Suit.Spades));
        Assert.False(LegalMoveValidator.IsLegalMove(hand, hand[1], Suit.Diamonds, winner, Suit.Spades));
    }

    [Fact]
    public void IsLegalMove_HasHigherLeadSuit_MustPlayHigher()
    {
        var hand = new List<Card>
        {
            new Card((byte)8, 0),
            new Card((byte)24, 0)
        };
        var winner = new Card((byte)16, 0);
        Assert.False(LegalMoveValidator.IsLegalMove(hand, hand[0], Suit.Diamonds, winner, Suit.Spades));
        Assert.True(LegalMoveValidator.IsLegalMove(hand, hand[1], Suit.Diamonds, winner, Suit.Spades));
    }

    [Fact]
    public void IsLegalMove_NoLeadSuitHasTrump_MustPlayTrump()
    {
        var hand = new List<Card>
        {
            new Card((byte)0, 0),
            new Card((byte)1, 0)
        };
        var winner = new Card((byte)10, 0);
        Assert.True(LegalMoveValidator.IsLegalMove(hand, hand[0], Suit.Hearts, winner, Suit.Diamonds));
        Assert.False(LegalMoveValidator.IsLegalMove(hand, hand[1], Suit.Hearts, winner, Suit.Diamonds));
    }

    [Fact]
    public void IsLegalMove_TrumpIsWinning_MustOverTrumpIfPossible()
    {
        var hand = new List<Card>
        {
            new Card((byte)8, 0),
            new Card((byte)24, 0)
        };
        var winner = new Card((byte)16, 0);
        Assert.False(LegalMoveValidator.IsLegalMove(hand, hand[0], Suit.Clubs, winner, Suit.Diamonds));
        Assert.True(LegalMoveValidator.IsLegalMove(hand, hand[1], Suit.Clubs, winner, Suit.Diamonds));
    }

    [Fact]
    public void IsLegalMove_NoLeadSuitNoTrump_AnyCardLegal()
    {
        var hand = new List<Card>
        {
            new Card((byte)1, 0),
            new Card((byte)5, 0)
        };
        var winner = new Card((byte)9, 0);
        Assert.True(LegalMoveValidator.IsLegalMove(hand, hand[0], Suit.Hearts, winner, Suit.Diamonds));
        Assert.True(LegalMoveValidator.IsLegalMove(hand, hand[1], Suit.Hearts, winner, Suit.Diamonds));
    }

    #endregion

    #region LegalMoveGenerator Tests

    [Fact]
    public void GetLegalMoves_NoCurrentWinner_ReturnsAllCards()
    {
        var hand = new List<Card>
        {
            new Card((byte)0, 0),
            new Card((byte)1, 0)
        };
        var legal = LegalMoveGenerator.GetLegalMoves(hand, Suit.Diamonds, null, Suit.Spades);
        Assert.Equal(2, legal.Count);
    }

    [Fact]
    public void GetLegalMoves_HasLeadSuit_ReturnsOnlyLeadSuit()
    {
        var hand = new List<Card>
        {
            new Card((byte)0, 0),
            new Card((byte)1, 0),
            new Card((byte)8, 0)
        };
        var winner = new Card((byte)12, 0);
        var legal = LegalMoveGenerator.GetLegalMoves(hand, Suit.Diamonds, winner, Suit.Spades);
        Assert.Equal(2, legal.Count);
        Assert.All(legal, c => Assert.Equal(Suit.Diamonds, c.Suit));
    }

    [Fact]
    public void GetLegalMoves_HasHigherLeadSuit_ReturnsOnlyHigherCards()
    {
        var hand = new List<Card>
        {
            new Card((byte)8, 0),
            new Card((byte)24, 0),
            new Card((byte)16, 0)
        };
        var winner = new Card((byte)16, 0);
        var legal = LegalMoveGenerator.GetLegalMoves(hand, Suit.Diamonds, winner, Suit.Spades);
        Assert.Single(legal);
        Assert.Equal((byte)24, legal[0].CardId);
    }

    [Fact]
    public void GetLegalMoves_NoLeadSuitHasTrump_ReturnsOnlyTrump()
    {
        var hand = new List<Card>
        {
            new Card((byte)0, 0),
            new Card((byte)4, 0),
            new Card((byte)1, 0)
        };
        var winner = new Card((byte)10, 0);
        var legal = LegalMoveGenerator.GetLegalMoves(hand, Suit.Hearts, winner, Suit.Diamonds);
        Assert.Equal(2, legal.Count);
        Assert.All(legal, c => Assert.Equal(Suit.Diamonds, c.Suit));
    }

    [Fact]
    public void GetLegalMoves_TrumpWinningNoHigherTrump_ReturnsAllTrump()
    {
        var hand = new List<Card>
        {
            new Card((byte)8, 0),
            new Card((byte)1, 0),
            new Card((byte)12, 0)
        };
        var winner = new Card((byte)24, 0);
        var legal = LegalMoveGenerator.GetLegalMoves(hand, Suit.Hearts, winner, Suit.Diamonds);
        Assert.Equal(2, legal.Count);
        Assert.All(legal, c => Assert.Equal(Suit.Diamonds, c.Suit));
    }

    [Fact]
    public void GetLegalMoves_NoLeadSuitNoTrump_ReturnsAllCards()
    {
        var hand = new List<Card>
        {
            new Card((byte)1, 0),
            new Card((byte)5, 0)
        };
        var winner = new Card((byte)9, 0);
        var legal = LegalMoveGenerator.GetLegalMoves(hand, Suit.Hearts, winner, Suit.Diamonds);
        Assert.Equal(2, legal.Count);
    }

    #endregion
}
