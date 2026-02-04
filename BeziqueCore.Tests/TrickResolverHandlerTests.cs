using BeziqueCore;

public class TrickResolverHandlerTests
{
    [Fact]
    public void ResolveTrick_ReturnsWinnerId()
    {
        var players = new[]
        {
            new Player(0) { WonPile = new List<Card>() },
            new Player(1) { WonPile = new List<Card>() }
        };
        var playedCards = new List<Card>
        {
            new Card((byte)0, 0),
            new Card((byte)1, 0)
        };
        var playerIndices = new[] { 0, 1 };

        int winnerId = TrickResolverHandler.ResolveTrick(playedCards, players, playerIndices, Suit.Spades, false);

        Assert.Equal(0, winnerId);
    }

    [Fact]
    public void ResolveTrick_TrumpSeven_AwardsBonus()
    {
        var players = new[]
        {
            new Player(0) { RoundScore = 100 },
            new Player(1) { RoundScore = 100 }
        };
        var playedCards = new List<Card>
        {
            new Card((byte)0, 0),
            new Card((byte)4, 0)
        };
        var playerIndices = new[] { 0, 1 };

        TrickResolverHandler.ResolveTrick(playedCards, players, playerIndices, Suit.Diamonds, false);

        Assert.Equal(110, players[0].RoundScore);
    }

    [Fact]
    public void ResolveTrick_FinalTrickWithTrumpSeven_Awards20Points()
    {
        var players = new[]
        {
            new Player(0) { RoundScore = 100 },
            new Player(1) { RoundScore = 100 }
        };
        var playedCards = new List<Card>
        {
            new Card((byte)0, 0),
            new Card((byte)7, 0)
        };
        var playerIndices = new[] { 0, 1 };

        TrickResolverHandler.ResolveTrick(playedCards, players, playerIndices, Suit.Diamonds, true);

        Assert.Equal(120, players[0].RoundScore);
    }

    [Fact]
    public void ResolveTrick_FinalTrickWithoutTrumpSeven_Awards10Points()
    {
        var players = new[]
        {
            new Player(0) { RoundScore = 100 },
            new Player(1) { RoundScore = 100 }
        };
        var playedCards = new List<Card>
        {
            new Card((byte)0, 0),
            new Card((byte)8, 0)
        };
        var playerIndices = new[] { 0, 1 };

        TrickResolverHandler.ResolveTrick(playedCards, players, playerIndices, Suit.Diamonds, true);

        Assert.Equal(110, players[0].RoundScore);
    }

    [Fact]
    public void ResolveTrick_MovesCardsToWinnerWonPile()
    {
        var players = new[]
        {
            new Player(0) { WonPile = new List<Card>() },
            new Player(1) { WonPile = new List<Card>() }
        };
        var playedCards = new List<Card>
        {
            new Card((byte)0, 0),
            new Card((byte)1, 0)
        };
        var playerIndices = new[] { 0, 1 };

        TrickResolverHandler.ResolveTrick(playedCards, players, playerIndices, Suit.Spades, false);

        Assert.Equal(2, players[0].WonPile.Count);
        Assert.Empty(players[1].WonPile);
    }

    [Fact]
    public void ResolveTrick_FourPlayers_ReturnsCorrectWinner()
    {
        var players = new[]
        {
            new Player(0) { WonPile = new List<Card>() },
            new Player(1) { WonPile = new List<Card>() },
            new Player(2) { WonPile = new List<Card>() },
            new Player(3) { WonPile = new List<Card>() }
        };
        var playedCards = new List<Card>
        {
            new Card((byte)24, 0),
            new Card((byte)28, 0),
            new Card((byte)25, 0),
            new Card((byte)29, 0)
        };
        var playerIndices = new[] { 0, 1, 2, 3 };

        int winnerId = TrickResolverHandler.ResolveTrick(playedCards, players, playerIndices, Suit.Diamonds, false);

        Assert.Equal(1, winnerId);
    }

    [Fact]
    public void ResolveTrick_TwoTrumpSeven_AwardsBothPlayers()
    {
        var players = new[]
        {
            new Player(0) { RoundScore = 100 },
            new Player(1) { RoundScore = 100 }
        };
        var playedCards = new List<Card>
        {
            new Card((byte)0, 0),
            new Card((byte)0, 1)
        };
        var playerIndices = new[] { 0, 1 };

        TrickResolverHandler.ResolveTrick(playedCards, players, playerIndices, Suit.Diamonds, false);

        Assert.Equal(110, players[0].RoundScore);
        Assert.Equal(110, players[1].RoundScore);
    }

    [Fact]
    public void ResolveTrick_NonFinalTrick_NoLastTrickBonus()
    {
        var players = new[]
        {
            new Player(0) { RoundScore = 100 }
        };
        var playedCards = new List<Card> { new Card((byte)0, 0) };
        var playerIndices = new[] { 0 };

        int beforeScore = players[0].RoundScore;
        TrickResolverHandler.ResolveTrick(playedCards, players, playerIndices, Suit.Spades, false);

        Assert.Equal(beforeScore, players[0].RoundScore);
    }
}
