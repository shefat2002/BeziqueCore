using BeziqueCore;

public class L9PlayStateHandlerTests
{
    [Fact]
    public void ValidateAndPlayCardPhase2_LegalMove_ReturnsTrueAndAddsToPlayed()
    {
        var player = new Player(0) { Hand = new List<Card> { new Card((byte)0, 0) } };
        var playedCards = new List<Card>();
        var leadSuit = Suit.Diamonds;

        bool result = L9PlayStateHandler.ValidateAndPlayCardPhase2(player, player.Hand[0], playedCards, 0, leadSuit, null, Suit.Spades);

        Assert.True(result);
        Assert.Single(playedCards);
        Assert.Empty(player.Hand);
    }

    [Fact]
    public void ValidateAndPlayCardPhase2_MustFollowSuit_ReturnsFalseForOffSuit()
    {
        var player = new Player(0)
        {
            Hand = new List<Card>
            {
                new Card((byte)0, 0),
                new Card((byte)1, 0)
            }
        };
        var playedCards = new List<Card>();
        var leadSuit = Suit.Diamonds;
        var currentWinner = new Card((byte)8, 0);

        bool result = L9PlayStateHandler.ValidateAndPlayCardPhase2(player, player.Hand[1], playedCards, 0, leadSuit, currentWinner, Suit.Spades);

        Assert.False(result);
        Assert.Empty(playedCards);
    }

    [Fact]
    public void ValidateAndPlayCardPhase2_MustPlayHigherIfCan_ReturnsFalseForLower()
    {
        var player = new Player(0)
        {
            Hand = new List<Card>
            {
                new Card((byte)8, 0),
                new Card((byte)24, 0)
            }
        };
        var playedCards = new List<Card>();
        var currentWinner = new Card((byte)16, 0);
        var leadSuit = Suit.Diamonds;

        bool result = L9PlayStateHandler.ValidateAndPlayCardPhase2(player, player.Hand[0], playedCards, 0, leadSuit, currentWinner, Suit.Spades);

        Assert.False(result);
        Assert.Empty(playedCards);
    }

    [Fact]
    public void ValidateAndPlayCardPhase2_NoLeadSuitHasTrump_MustPlayTrump()
    {
        var player = new Player(0)
        {
            Hand = new List<Card>
            {
                new Card((byte)0, 0),
                new Card((byte)1, 0)
            }
        };
        var playedCards = new List<Card>();
        var leadSuit = Suit.Hearts;
        var currentWinner = new Card((byte)8, 0);
        var trump = Suit.Diamonds;

        bool result = L9PlayStateHandler.ValidateAndPlayCardPhase2(player, player.Hand[1], playedCards, 0, leadSuit, currentWinner, trump);

        Assert.False(result);
        Assert.Empty(playedCards);
    }

    [Fact]
    public void ValidateAndPlayCardPhase2_MustOverTrumpIfCan_ReturnsFalseForLower()
    {
        var player = new Player(0)
        {
            Hand = new List<Card>
            {
                new Card((byte)8, 0),
                new Card((byte)24, 0)
            }
        };
        var playedCards = new List<Card>();
        var currentWinner = new Card((byte)16, 0);
        var leadSuit = Suit.Hearts;
        var trump = Suit.Diamonds;

        bool result = L9PlayStateHandler.ValidateAndPlayCardPhase2(player, player.Hand[0], playedCards, 0, leadSuit, currentWinner, trump);

        Assert.False(result);
        Assert.Empty(playedCards);
    }

    [Fact]
    public void ValidateAndPlayCardPhase2_NoLeadSuitNoTrump_AnyCardLegal()
    {
        var player = new Player(0)
        {
            Hand = new List<Card>
            {
                new Card((byte)1, 0),
                new Card((byte)5, 0)
            }
        };
        var playedCards = new List<Card>();
        var leadSuit = Suit.Diamonds;
        var trump = Suit.Hearts;

        bool result = L9PlayStateHandler.ValidateAndPlayCardPhase2(player, player.Hand[0], playedCards, 0, leadSuit, null, trump);

        Assert.True(result);
        Assert.Single(playedCards);
        Assert.Single(player.Hand);
    }

    [Fact]
    public void ValidateAndPlayCardPhase2_ValidTrump_ReturnsTrue()
    {
        var player = new Player(0)
        {
            Hand = new List<Card>
            {
                new Card((byte)1, 0),
                new Card((byte)0, 0)
            }
        };
        var playedCards = new List<Card>();
        var leadSuit = Suit.Hearts;
        var trump = Suit.Diamonds;

        bool result = L9PlayStateHandler.ValidateAndPlayCardPhase2(player, player.Hand[0], playedCards, 0, leadSuit, null, trump);

        Assert.True(result);
        Assert.Single(playedCards);
        Assert.Single(player.Hand);
    }
}
