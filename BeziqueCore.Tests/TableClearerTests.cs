using BeziqueCore;

public class TableClearerTests
{
    [Fact]
    public void ReturnAllTableCardsToHand_SinglePlayerWithTableCards_MovesAllToHand()
    {
        var players = new[]
        {
            new Player(0) { Hand = new List<Card>(), TableCards = new List<Card> { new Card((byte)10, 0), new Card((byte)11, 1) } },
            new Player(1) { Hand = new List<Card>(), TableCards = new List<Card>() }
        };

        TableClearer.ReturnAllTableCardsToHand(players);

        Assert.Equal(2, players[0].Hand.Count);
        Assert.Empty(players[0].TableCards);
        Assert.Equal((byte)10, players[0].Hand[0].CardId);
        Assert.Equal((byte)11, players[0].Hand[1].CardId);
    }

    [Fact]
    public void ReturnAllTableCardsToHand_MultiplePlayersWithTableCards_MovesAllToHand()
    {
        var players = new[]
        {
            new Player(0) { Hand = new List<Card> { new Card((byte)5, 0) }, TableCards = new List<Card> { new Card((byte)10, 1) } },
            new Player(1) { Hand = new List<Card> { new Card((byte)6, 0) }, TableCards = new List<Card> { new Card((byte)11, 1), new Card((byte)12, 2) } }
        };

        TableClearer.ReturnAllTableCardsToHand(players);

        Assert.Equal(2, players[0].Hand.Count);
        Assert.Empty(players[0].TableCards);
        Assert.Equal(3, players[1].Hand.Count);
        Assert.Empty(players[1].TableCards);
    }

    [Fact]
    public void ReturnAllTableCardsToHand_FourPlayersWithTableCards_MovesAllToHand()
    {
        var players = new[]
        {
            new Player(0) { Hand = new List<Card>(), TableCards = new List<Card> { new Card((byte)10, 0) } },
            new Player(1) { Hand = new List<Card>(), TableCards = new List<Card> { new Card((byte)11, 1) } },
            new Player(2) { Hand = new List<Card>(), TableCards = new List<Card> { new Card((byte)12, 2) } },
            new Player(3) { Hand = new List<Card>(), TableCards = new List<Card> { new Card((byte)13, 3) } }
        };

        TableClearer.ReturnAllTableCardsToHand(players);

        Assert.Single(players[0].Hand);
        Assert.Empty(players[0].TableCards);
        Assert.Single(players[1].Hand);
        Assert.Empty(players[1].TableCards);
        Assert.Single(players[2].Hand);
        Assert.Empty(players[2].TableCards);
        Assert.Single(players[3].Hand);
        Assert.Empty(players[3].TableCards);
    }

    [Fact]
    public void ReturnAllTableCardsToHand_AllPlayersEmptyTableCards_DoesNothing()
    {
        var players = new[]
        {
            new Player(0) { Hand = new List<Card> { new Card((byte)10, 0) }, TableCards = new List<Card>() },
            new Player(1) { Hand = new List<Card> { new Card((byte)11, 1) }, TableCards = new List<Card>() }
        };

        TableClearer.ReturnAllTableCardsToHand(players);

        Assert.Single(players[0].Hand);
        Assert.Empty(players[0].TableCards);
        Assert.Single(players[1].Hand);
        Assert.Empty(players[1].TableCards);
    }

    [Fact]
    public void ReturnAllTableCardsToHand_AppendsToExistingHand()
    {
        var players = new[]
        {
            new Player(0) { Hand = new List<Card> { new Card((byte)5, 0) }, TableCards = new List<Card> { new Card((byte)10, 1) } },
            new Player(1) { Hand = new List<Card> { new Card((byte)6, 0) }, TableCards = new List<Card>() }
        };

        TableClearer.ReturnAllTableCardsToHand(players);

        Assert.Equal(2, players[0].Hand.Count);
        Assert.Equal((byte)5, players[0].Hand[0].CardId);
        Assert.Equal((byte)10, players[0].Hand[1].CardId);
        Assert.Empty(players[0].TableCards);
    }

    [Fact]
    public void ReturnAllTableCardsToHand_PreservesMeldHistory()
    {
        var players = new[]
        {
            new Player(0)
            {
                Hand = new List<Card>(),
                TableCards = new List<Card> { new Card((byte)10, 0) },
                MeldHistory = new Dictionary<MeldType, List<Card>>
                {
                    { MeldType.Bezique, new List<Card> { new Card((byte)23, 1) } }
                }
            }
        };

        TableClearer.ReturnAllTableCardsToHand(players);

        Assert.NotEmpty(players[0].MeldHistory);
        Assert.True(players[0].MeldHistory.ContainsKey(MeldType.Bezique));
    }
}
