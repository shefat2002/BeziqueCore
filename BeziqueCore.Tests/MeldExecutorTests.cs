using BeziqueCore;

public class MeldExecutorTests
{
    [Fact]
    public void TryExecuteMeld_ReturnsTrue_WhenValidTrumpSeven()
    {
        var player = new Player(0);
        player.Hand.Add(new Card(3, 0));
        var cards = new[] { player.Hand[0] };

        var result = MeldExecutor.TryExecuteMeld(player, cards, MeldType.TrumpSeven, Suit.Spades);

        Assert.True(result);
        Assert.Equal(10, player.RoundScore);
        Assert.Empty(player.Hand);
        Assert.Single(player.TableCards);
    }

    [Fact]
    public void TryExecuteMeld_ReturnsTrue_WhenValidTrumpMarriage()
    {
        var player = new Player(0);
        player.Hand.Add(new Card(22, 0));
        player.Hand.Add(new Card(18, 0));
        var cards = new[] { player.Hand[0], player.Hand[1] };

        var result = MeldExecutor.TryExecuteMeld(player, cards, MeldType.TrumpMarriage, Suit.Hearts);

        Assert.True(result);
        Assert.Equal(40, player.RoundScore);
        Assert.Empty(player.Hand);
        Assert.Equal(2, player.TableCards.Count);
    }

    [Fact]
    public void TryExecuteMeld_ReturnsFalse_WhenInvalidMeld()
    {
        var player = new Player(0);
        player.Hand.Add(new Card(10, 0));
        var cards = new[] { player.Hand[0] };

        var result = MeldExecutor.TryExecuteMeld(player, cards, MeldType.FourAces, Suit.Diamonds);

        Assert.False(result);
        Assert.Single(player.Hand);
        Assert.Empty(player.TableCards);
    }

    [Fact]
    public void TryExecuteMeld_ReturnsFalse_WhenCardAlreadyUsed()
    {
        var player = new Player(0);
        var card = new Card(28, 0);
        player.Hand.Add(card);
        player.MeldHistory[MeldType.FourAces] = new List<Card> { card };
        var cards = new[] { card };

        var result = MeldExecutor.TryExecuteMeld(player, cards, MeldType.FourAces, Suit.Diamonds);

        Assert.False(result);
    }

    [Fact]
    public void TryExecuteMeld_UpdatesMeldHistory()
    {
        var player = new Player(0);
        player.Hand.Add(new Card(3, 0));
        var cards = new[] { player.Hand[0] };

        MeldExecutor.TryExecuteMeld(player, cards, MeldType.TrumpSeven, Suit.Spades);

        Assert.True(player.MeldHistory.ContainsKey(MeldType.TrumpSeven));
        Assert.Single(player.MeldHistory[MeldType.TrumpSeven]);
    }

    [Fact]
    public void TryExecuteMeld_AllowsSameCardInDifferentMeld()
    {
        var player = new Player(0);
        var king = new Card(20, 0);
        var queen = new Card(16, 0);
        player.Hand.Add(king);
        player.Hand.Add(queen);
        player.MeldHistory[MeldType.TrumpMarriage] = new List<Card> { king, queen };
        var cards = new[] { king, queen };

        var result = MeldExecutor.TryExecuteMeld(player, cards, MeldType.NonTrumpMarriage, Suit.Spades);

        Assert.True(result);
    }

    [Fact]
    public void TryExecuteMeld_AwardsCorrectPoints_ForAllMeldTypes()
    {
        var player = new Player(0);

        Assert.Equal(10, MeldDefinitions.GetPoints(MeldType.TrumpSeven));
        Assert.Equal(40, MeldDefinitions.GetPoints(MeldType.TrumpMarriage));
        Assert.Equal(20, MeldDefinitions.GetPoints(MeldType.NonTrumpMarriage));
        Assert.Equal(40, MeldDefinitions.GetPoints(MeldType.Bezique));
        Assert.Equal(500, MeldDefinitions.GetPoints(MeldType.DoubleBezique));
        Assert.Equal(250, MeldDefinitions.GetPoints(MeldType.TrumpRun));
        Assert.Equal(100, MeldDefinitions.GetPoints(MeldType.FourAces));
        Assert.Equal(80, MeldDefinitions.GetPoints(MeldType.FourKings));
        Assert.Equal(60, MeldDefinitions.GetPoints(MeldType.FourQueens));
        Assert.Equal(40, MeldDefinitions.GetPoints(MeldType.FourJacks));
    }

    [Fact]
    public void TryExecuteMeld_WithList_ReturnsTrue_WhenValid()
    {
        var player = new Player(0);
        var cards = new List<Card> { new Card(3, 0) };
        player.Hand.Add(cards[0]);

        var result = MeldExecutor.TryExecuteMeld(player, cards, MeldType.TrumpSeven, Suit.Spades);

        Assert.True(result);
        Assert.Equal(10, player.RoundScore);
    }

    [Fact]
    public void TryExecuteMeld_WithList_ReturnsFalse_WhenInvalid()
    {
        var player = new Player(0);
        var cards = new List<Card> { new Card(10, 0) };
        player.Hand.Add(cards[0]);

        var result = MeldExecutor.TryExecuteMeld(player, cards, MeldType.FourAces, Suit.Diamonds);

        Assert.False(result);
    }
}
