using BeziqueCore;

public class FinalDrawCoordinatorTests
{
    [Fact]
    public void ExecuteFinalDraw_TwoPlayers_WinnerGetsHiddenCard_LoserGetsTrumpCard()
    {
        var players = new[]
        {
            new Player(0) { Hand = new List<Card>() },
            new Player(1) { Hand = new List<Card>() }
        };
        var trumpCard = new Card((byte)0, 0);
        var drawDeck = new Stack<Card>();
        drawDeck.Push(new Card((byte)31, 3));

        FinalDrawCoordinator.ExecuteFinalDraw(players, winnerId: 0, trumpCard, drawDeck);

        Assert.Equal(1, players[0].Hand.Count);
        Assert.Equal((byte)31, players[0].Hand[0].CardId);
        Assert.Equal(1, players[1].Hand.Count);
        Assert.Equal((byte)0, players[1].Hand[0].CardId);
    }

    [Fact]
    public void ExecuteFinalDraw_TwoPlayers_WinnerIsPlayer1_LoserGetsTrumpCard()
    {
        var players = new[]
        {
            new Player(0) { Hand = new List<Card>() },
            new Player(1) { Hand = new List<Card>() }
        };
        var trumpCard = new Card((byte)0, 0);
        var drawDeck = new Stack<Card>();
        drawDeck.Push(new Card((byte)31, 3));

        FinalDrawCoordinator.ExecuteFinalDraw(players, winnerId: 1, trumpCard, drawDeck);

        Assert.Equal(1, players[1].Hand.Count);
        Assert.Equal((byte)31, players[1].Hand[0].CardId);
        Assert.Equal(1, players[0].Hand.Count);
        Assert.Equal((byte)0, players[0].Hand[0].CardId);
    }

    [Fact]
    public void ExecuteFinalDraw_FourPlayers_WinnerGetsHiddenCard_AllLosersGetTrumpCard()
    {
        var players = new[]
        {
            new Player(0) { Hand = new List<Card>() },
            new Player(1) { Hand = new List<Card>() },
            new Player(2) { Hand = new List<Card>() },
            new Player(3) { Hand = new List<Card>() }
        };
        var trumpCard = new Card((byte)0, 0);
        var drawDeck = new Stack<Card>();
        drawDeck.Push(new Card((byte)31, 3));

        FinalDrawCoordinator.ExecuteFinalDraw(players, winnerId: 0, trumpCard, drawDeck);

        Assert.Equal(1, players[0].Hand.Count);
        Assert.Equal((byte)31, players[0].Hand[0].CardId);
        Assert.Equal(1, players[1].Hand.Count);
        Assert.Equal((byte)0, players[1].Hand[0].CardId);
        Assert.Equal(1, players[2].Hand.Count);
        Assert.Equal((byte)0, players[2].Hand[0].CardId);
        Assert.Equal(1, players[3].Hand.Count);
        Assert.Equal((byte)0, players[3].Hand[0].CardId);
    }

    [Fact]
    public void ExecuteFinalDraw_FourPlayers_WinnerIsPlayer2_VerifyCardDistribution()
    {
        var players = new[]
        {
            new Player { PlayerID = 0, Hand = new List<Card>() },
            new Player { PlayerID = 1, Hand = new List<Card>() },
            new Player { PlayerID = 2, Hand = new List<Card>() },
            new Player { PlayerID = 3, Hand = new List<Card>() }
        };
        var trumpCard = new Card((byte)5, 1);
        var drawDeck = new Stack<Card>();
        drawDeck.Push(new Card((byte)28, 2));

        FinalDrawCoordinator.ExecuteFinalDraw(players, winnerId: 2, trumpCard, drawDeck);

        Assert.Equal(1, players[2].Hand.Count);
        Assert.Equal((byte)28, players[2].Hand[0].CardId);
        Assert.All(players.Where(p => p.PlayerID != 2), p => Assert.Single(p.Hand));
        Assert.All(players.Where(p => p.PlayerID != 2), p => Assert.Equal((byte)5, p.Hand[0].CardId));
    }

    [Fact]
    public void ExecuteFinalDraw_AppendsToExistingHand()
    {
        var players = new[]
        {
            new Player { PlayerID = 0, Hand = new List<Card> { new Card((byte)10, 0) } },
            new Player { PlayerID = 1, Hand = new List<Card> { new Card((byte)11, 1) } }
        };
        var trumpCard = new Card((byte)0, 0);
        var drawDeck = new Stack<Card>();
        drawDeck.Push(new Card((byte)31, 3));

        FinalDrawCoordinator.ExecuteFinalDraw(players, winnerId: 0, trumpCard, drawDeck);

        Assert.Equal(2, players[0].Hand.Count);
        Assert.Equal((byte)10, players[0].Hand[0].CardId);
        Assert.Equal((byte)31, players[0].Hand[1].CardId);
        Assert.Equal(2, players[1].Hand.Count);
        Assert.Equal((byte)11, players[1].Hand[0].CardId);
        Assert.Equal((byte)0, players[1].Hand[1].CardId);
    }
}
