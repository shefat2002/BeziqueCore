using BeziqueCore;

public class Phase2MoveValidatorTests
{
    #region HasLeadSuit Tests

    [Fact]
    public void HasLeadSuit_ReturnsTrue_WhenHandContainsLeadSuit()
    {
        var hand = new List<Card> { new Card((byte)0, 0), new Card((byte)4, 0) };
        Assert.True(Phase2MoveValidator.HasLeadSuit(hand, Suit.Diamonds));
    }

    [Fact]
    public void HasLeadSuit_ReturnsFalse_WhenHandDoesNotContainLeadSuit()
    {
        var hand = new List<Card> { new Card((byte)1, 0), new Card((byte)5, 0) };
        Assert.False(Phase2MoveValidator.HasLeadSuit(hand, Suit.Diamonds));
    }

    #endregion

    #region HasHigherCard Tests

    [Fact]
    public void HasHigherCard_ReturnsTrue_WhenHandHasHigherRankOfSameSuit()
    {
        var hand = new List<Card> { new Card((byte)28, 0) };
        var winner = new Card((byte)24, 0);
        Assert.True(Phase2MoveValidator.HasHigherCard(hand, Suit.Diamonds, winner));
    }

    [Fact]
    public void HasHigherCard_ReturnsFalse_WhenHandHasNoHigherRankOfSameSuit()
    {
        var hand = new List<Card> { new Card((byte)8, 0) };
        var winner = new Card((byte)24, 0);
        Assert.False(Phase2MoveValidator.HasHigherCard(hand, Suit.Diamonds, winner));
    }

    [Fact]
    public void HasHigherCard_ReturnsTrue_WhenCurrentWinnerIsJoker()
    {
        var hand = new List<Card> { new Card((byte)0, 0) };
        var joker = new Card((byte)32, 0);
        Assert.True(Phase2MoveValidator.HasHigherCard(hand, Suit.Diamonds, joker));
    }

    [Fact]
    public void HasHigherCard_ReturnsFalse_WhenHandIsEmpty()
    {
        var hand = new List<Card>();
        var winner = new Card((byte)0, 0);
        Assert.False(Phase2MoveValidator.HasHigherCard(hand, Suit.Diamonds, winner));
    }

    #endregion

    #region HasTrumpSuit Tests

    [Fact]
    public void HasTrumpSuit_ReturnsTrue_WhenHandContainsTrump()
    {
        var hand = new List<Card> { new Card((byte)0, 0), new Card((byte)1, 0) };
        Assert.True(Phase2MoveValidator.HasTrumpSuit(hand, Suit.Diamonds));
    }

    [Fact]
    public void HasTrumpSuit_ReturnsFalse_WhenHandDoesNotContainTrump()
    {
        var hand = new List<Card> { new Card((byte)1, 0), new Card((byte)6, 0) };
        Assert.False(Phase2MoveValidator.HasTrumpSuit(hand, Suit.Diamonds));
    }

    #endregion

    #region IsLegalMove Tests

    [Fact]
    public void IsLegalMove_NoCurrentWinner_AllCardsLegal()
    {
        var hand = new List<Card> { new Card((byte)0, 0), new Card((byte)1, 0) };
        Assert.True(Phase2MoveValidator.IsLegalMove(hand, hand[0], Suit.Diamonds, null, Suit.Spades));
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
        Assert.True(Phase2MoveValidator.IsLegalMove(hand, hand[0], Suit.Diamonds, winner, Suit.Spades));
        Assert.False(Phase2MoveValidator.IsLegalMove(hand, hand[1], Suit.Diamonds, winner, Suit.Spades));
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
        Assert.False(Phase2MoveValidator.IsLegalMove(hand, hand[0], Suit.Diamonds, winner, Suit.Spades));
        Assert.True(Phase2MoveValidator.IsLegalMove(hand, hand[1], Suit.Diamonds, winner, Suit.Spades));
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
        Assert.True(Phase2MoveValidator.IsLegalMove(hand, hand[0], Suit.Hearts, winner, Suit.Diamonds));
        Assert.False(Phase2MoveValidator.IsLegalMove(hand, hand[1], Suit.Hearts, winner, Suit.Diamonds));
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
        Assert.False(Phase2MoveValidator.IsLegalMove(hand, hand[0], Suit.Clubs, winner, Suit.Diamonds));
        Assert.True(Phase2MoveValidator.IsLegalMove(hand, hand[1], Suit.Clubs, winner, Suit.Diamonds));
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
        Assert.True(Phase2MoveValidator.IsLegalMove(hand, hand[0], Suit.Hearts, winner, Suit.Diamonds));
        Assert.True(Phase2MoveValidator.IsLegalMove(hand, hand[1], Suit.Hearts, winner, Suit.Diamonds));
    }

    #endregion
}
