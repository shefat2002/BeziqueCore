using BeziqueCore;

public class TrickResolverTests
{
    [Fact]
    public void ResolveTrick_ReturnsWinnerIndex_WhenTwoPlayers()
    {
        var players = new[] { new Player(0), new Player(1) };
        var playedCards = new[] { new Card(10, 0), new Card(20, 0) };

        var winnerIndex = TrickResolver.ResolveTrick(playedCards, players, Suit.Hearts);

        Assert.Equal(0, winnerIndex);
    }

    [Fact]
    public void ResolveTrick_MovesCardsToWinnerWonPile()
    {
        var players = new[] { new Player(0), new Player(1) };
        var playedCards = new[] { new Card(10, 0), new Card(20, 0) };

        TrickResolver.ResolveTrick(playedCards, players, Suit.Hearts);

        Assert.Equal(2, players[0].WonPile.Count);
        Assert.Empty(players[1].WonPile);
    }

    [Fact]
    public void ResolveTrick_AwardsTenPoints_WhenTrumpSevenPlayed()
    {
        var players = new[] { new Player(0), new Player(1) };
        var playedCards = new[] { new Card(3, 0), new Card(20, 0) };

        TrickResolver.ResolveTrick(playedCards, players, Suit.Spades);

        Assert.Equal(10, players[0].RoundScore);
        Assert.Equal(0, players[1].RoundScore);
    }

    [Fact]
    public void ResolveTrick_AwardsTenPointsToMultiplePlayers_WhenMultipleTrumpSevens()
    {
        var players = new[] { new Player(0), new Player(1), new Player(2) };
        var playedCards = new[] { new Card(3, 0), new Card(3, 1), new Card(20, 0) };

        TrickResolver.ResolveTrick(playedCards, players, Suit.Spades);

        Assert.Equal(10, players[0].RoundScore);
        Assert.Equal(10, players[1].RoundScore);
        Assert.Equal(0, players[2].RoundScore);
    }

    [Fact]
    public void ResolveTrick_DoesNotAwardPoints_WhenNoTrumpSeven()
    {
        var players = new[] { new Player(0), new Player(1) };
        var playedCards = new[] { new Card(10, 0), new Card(20, 0) };

        TrickResolver.ResolveTrick(playedCards, players, Suit.Spades);

        Assert.Equal(0, players[0].RoundScore);
        Assert.Equal(0, players[1].RoundScore);
    }

    [Fact]
    public void ResolveTrick_ReturnsNegativeOne_WhenNoCardsPlayed()
    {
        var players = new[] { new Player(0), new Player(1) };
        var playedCards = Array.Empty<Card>();

        var winnerIndex = TrickResolver.ResolveTrick(playedCards, players, Suit.Hearts);

        Assert.Equal(-1, winnerIndex);
    }

    [Fact]
    public void ResolveTrick_WithList_ReturnsWinnerIndex_WhenTwoPlayers()
    {
        var players = new[] { new Player(0), new Player(1) };
        IReadOnlyList<Card> playedCards = new List<Card> { new Card(10, 0), new Card(20, 0) };

        var winnerIndex = TrickResolver.ResolveTrick(playedCards, players, Suit.Hearts);

        Assert.Equal(0, winnerIndex);
    }

    [Fact]
    public void ResolveTrick_WithList_AwardsPoints_WhenTrumpSevenPlayed()
    {
        var players = new[] { new Player(0), new Player(1) };
        IReadOnlyList<Card> playedCards = new List<Card> { new Card(3, 0), new Card(20, 0) };

        TrickResolver.ResolveTrick(playedCards, players, Suit.Spades);

        Assert.Equal(10, players[0].RoundScore);
    }
}
