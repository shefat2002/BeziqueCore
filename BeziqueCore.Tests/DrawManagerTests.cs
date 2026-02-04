using BeziqueCore;

public class DrawManagerTests
{
    [Fact]
    public void ExecuteDraw_NormalDraw_TwoPlayers_WinnerDrawsFirstThenLoser()
    {
        var players = new[]
        {
            new Player(0) { Hand = new List<Card>() },
            new Player(1) { Hand = new List<Card>() }
        };
        var trumpCard = new Card((byte)0, 0);
        var drawDeck = new Stack<Card>();
        drawDeck.Push(new Card((byte)31, 3));
        drawDeck.Push(new Card((byte)30, 2));

        bool transitioned = DrawManager.ExecuteDraw(players, winnerId: 0, trumpCard, drawDeck, playerCount: 2);

        Assert.False(transitioned);
        Assert.Single(players[0].Hand);
        Assert.Single(players[1].Hand);
        Assert.Equal((byte)30, players[0].Hand[0].CardId);
        Assert.Equal((byte)31, players[1].Hand[0].CardId);
    }

    [Fact]
    public void ExecuteDraw_NormalDraw_FourPlayers_WinnerDrawsFirstThenOthers()
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
        drawDeck.Push(new Card((byte)30, 2));
        drawDeck.Push(new Card((byte)29, 1));
        drawDeck.Push(new Card((byte)28, 0));
        drawDeck.Push(new Card((byte)27, 3));

        bool transitioned = DrawManager.ExecuteDraw(players, winnerId: 1, trumpCard, drawDeck, playerCount: 4);

        Assert.False(transitioned);
        Assert.Single(players[1].Hand);
        Assert.Equal((byte)27, players[1].Hand[0].CardId);
        Assert.Equal((byte)28, players[0].Hand[0].CardId);
        Assert.Equal((byte)29, players[2].Hand[0].CardId);
        Assert.Equal((byte)30, players[3].Hand[0].CardId);
    }

    [Fact]
    public void ExecuteDraw_FinalDraw_TwoPlayers_ReturnsTrueAndClearsTable()
    {
        var players = new[]
        {
            new Player(0) { Hand = new List<Card>(), TableCards = new List<Card> { new Card((byte)10, 0) } },
            new Player(1) { Hand = new List<Card>(), TableCards = new List<Card> { new Card((byte)11, 1) } }
        };
        var trumpCard = new Card((byte)0, 0);
        var drawDeck = new Stack<Card>();
        drawDeck.Push(new Card((byte)31, 3));
        drawDeck.Push(new Card((byte)30, 2));

        bool transitioned = DrawManager.ExecuteDraw(players, winnerId: 0, trumpCard, drawDeck, playerCount: 2);

        Assert.True(transitioned);
        Assert.Equal(2, players[0].Hand.Count);
        Assert.Equal(2, players[1].Hand.Count);
        Assert.Empty(players[0].TableCards);
        Assert.Empty(players[1].TableCards);
        Assert.Contains(players[0].Hand, c => c.CardId == (byte)10);
        Assert.Contains(players[0].Hand, c => c.CardId == (byte)30);
        Assert.Contains(players[1].Hand, c => c.CardId == (byte)11);
        Assert.Contains(players[1].Hand, c => c.CardId == (byte)0);
    }

    [Fact]
    public void ExecuteDraw_FinalDraw_FourPlayers_ReturnsTrueAndClearsAllTables()
    {
        var players = new[]
        {
            new Player(0) { Hand = new List<Card>(), TableCards = new List<Card> { new Card((byte)10, 0) } },
            new Player(1) { Hand = new List<Card>(), TableCards = new List<Card> { new Card((byte)11, 1) } },
            new Player(2) { Hand = new List<Card>(), TableCards = new List<Card> { new Card((byte)12, 2) } },
            new Player(3) { Hand = new List<Card>(), TableCards = new List<Card> { new Card((byte)13, 3) } }
        };
        var trumpCard = new Card((byte)5, 1);
        var drawDeck = new Stack<Card>();
        drawDeck.Push(new Card((byte)31, 3));
        drawDeck.Push(new Card((byte)30, 2));
        drawDeck.Push(new Card((byte)29, 1));
        drawDeck.Push(new Card((byte)28, 0));

        bool transitioned = DrawManager.ExecuteDraw(players, winnerId: 2, trumpCard, drawDeck, playerCount: 4);

        Assert.True(transitioned);
        Assert.Equal(2, players[2].Hand.Count);
        Assert.Equal((byte)28, players[2].Hand[0].CardId);
        Assert.Contains(players[2].Hand, c => c.CardId == (byte)12);
        Assert.All(players.Where(p => p.PlayerID != 2), p => Assert.Equal(2, p.Hand.Count));
        Assert.All(players.Where(p => p.PlayerID != 2), p => Assert.Contains(p.Hand, c => c.CardId == (byte)5));
        Assert.All(players, p => Assert.Empty(p.TableCards));
    }

    [Fact]
    public void ExecuteDraw_EmptyDeck_NoCardsDrawn()
    {
        var players = new[]
        {
            new Player(0) { Hand = new List<Card>() },
            new Player(1) { Hand = new List<Card>() }
        };
        var trumpCard = new Card((byte)0, 0);
        var drawDeck = new Stack<Card>();

        bool transitioned = DrawManager.ExecuteDraw(players, winnerId: 0, trumpCard, drawDeck, playerCount: 2);

        Assert.False(transitioned);
        Assert.Empty(players[0].Hand);
        Assert.Empty(players[1].Hand);
    }

    [Fact]
    public void ExecuteDraw_AppendsToExistingHand()
    {
        var players = new[]
        {
            new Player(0) { Hand = new List<Card> { new Card((byte)5, 0) } },
            new Player(1) { Hand = new List<Card> { new Card((byte)6, 1) } }
        };
        var trumpCard = new Card((byte)0, 0);
        var drawDeck = new Stack<Card>();
        drawDeck.Push(new Card((byte)31, 3));
        drawDeck.Push(new Card((byte)30, 2));

        DrawManager.ExecuteDraw(players, winnerId: 0, trumpCard, drawDeck, playerCount: 2);

        Assert.Equal(2, players[0].Hand.Count);
        Assert.Equal(2, players[1].Hand.Count);
        Assert.Equal((byte)5, players[0].Hand[0].CardId);
        Assert.Equal((byte)30, players[0].Hand[1].CardId);
        Assert.Equal((byte)6, players[1].Hand[0].CardId);
        Assert.Equal((byte)31, players[1].Hand[1].CardId);
    }
}
