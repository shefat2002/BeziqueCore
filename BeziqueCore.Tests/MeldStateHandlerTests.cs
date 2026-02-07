using BeziqueCore;

public class MeldStateHandlerTests
{
    [Fact]
    public void DeclareMeld_TrumpSeven_ReturnsTrueAndAwardsPoints()
    {
        var seven = new Card((byte)0, 0);
        var player = new Player(0)
        {
            Hand = new List<Card> { seven },
            RoundScore = 100
        };

        bool result = MeldStateHandler.DeclareMeld(player, new[] { seven }, MeldType.TrumpSeven, Suit.Diamonds);

        Assert.True(result);
        Assert.Equal(110, player.RoundScore);
        Assert.Single(player.TableCards);
        Assert.Empty(player.Hand);
    }

    [Fact]
    public void DeclareMeld_TrumpMarriage_ReturnsTrueAndAwardsPoints()
    {
        var king = new Card((byte)20, 0);
        var queen = new Card((byte)16, 0);
        var player = new Player(0)
        {
            Hand = new List<Card> { king, queen },
            RoundScore = 100
        };

        bool result = MeldStateHandler.DeclareMeld(player, new[] { king, queen }, MeldType.TrumpMarriage, Suit.Diamonds);

        Assert.True(result);
        Assert.Equal(140, player.RoundScore);
        Assert.Equal(2, player.TableCards.Count);
        Assert.Empty(player.Hand);
    }

    [Fact]
    public void DeclareMeld_NonTrumpMarriage_ReturnsTrueAndAwardsPoints()
    {
        var king = new Card((byte)21, 1);
        var queen = new Card((byte)17, 1);
        var player = new Player(0)
        {
            Hand = new List<Card> { king, queen },
            RoundScore = 100
        };

        bool result = MeldStateHandler.DeclareMeld(player, new[] { king, queen }, MeldType.NonTrumpMarriage, Suit.Diamonds);

        Assert.True(result);
        Assert.Equal(120, player.RoundScore);
        Assert.Equal(2, player.TableCards.Count);
        Assert.Empty(player.Hand);
    }

    [Fact]
    public void DeclareMeld_Bezique_ReturnsTrueAndAwardsPoints()
    {
        var queen = new Card((byte)19, 3);
        var jack = new Card((byte)12, 0);
        var player = new Player(0)
        {
            Hand = new List<Card> { queen, jack },
            RoundScore = 100
        };

        bool result = MeldStateHandler.DeclareMeld(player, new[] { queen, jack }, MeldType.Bezique, Suit.Hearts);

        Assert.True(result);
        Assert.Equal(140, player.RoundScore);
        Assert.Equal(2, player.TableCards.Count);
        Assert.Empty(player.Hand);
    }

    [Fact]
    public void DeclareMeld_DoubleBezique_ReturnsTrueAndAwardsPoints()
    {
        var queen1 = new Card((byte)19, 0);
        var queen2 = new Card((byte)19, 1);
        var jack1 = new Card((byte)12, 0);
        var jack2 = new Card((byte)12, 1);
        var player = new Player(0)
        {
            Hand = new List<Card> { queen1, queen2, jack1, jack2 },
            RoundScore = 100
        };

        bool result = MeldStateHandler.DeclareMeld(player, new[] { queen1, queen2, jack1, jack2 }, MeldType.DoubleBezique, Suit.Hearts);

        Assert.True(result);
        Assert.Equal(600, player.RoundScore);
        Assert.Equal(4, player.TableCards.Count);
        Assert.Empty(player.Hand);
    }

    [Fact]
    public void DeclareMeld_TrumpRun_ReturnsTrueAndAwardsPoints()
    {
        var ace = new Card((byte)28, 0);
        var ten = new Card((byte)24, 0);
        var king = new Card((byte)20, 0);
        var queen = new Card((byte)16, 0);
        var jack = new Card((byte)12, 0);
        var player = new Player(0)
        {
            Hand = new List<Card> { ace, ten, king, queen, jack },
            RoundScore = 100
        };

        bool result = MeldStateHandler.DeclareMeld(player, new[] { ace, ten, king, queen, jack }, MeldType.TrumpRun, Suit.Diamonds);

        Assert.True(result);
        Assert.Equal(350, player.RoundScore);
        Assert.Equal(5, player.TableCards.Count);
        Assert.Empty(player.Hand);
    }

    [Fact]
    public void DeclareMeld_FourAces_ReturnsTrueAndAwardsPoints()
    {
        var ace1 = new Card((byte)28, 0);
        var ace2 = new Card((byte)29, 1);
        var ace3 = new Card((byte)30, 2);
        var ace4 = new Card((byte)31, 3);
        var player = new Player(0)
        {
            Hand = new List<Card> { ace1, ace2, ace3, ace4 },
            RoundScore = 100
        };

        bool result = MeldStateHandler.DeclareMeld(player, new[] { ace1, ace2, ace3, ace4 }, MeldType.FourAces, Suit.Diamonds);

        Assert.True(result);
        Assert.Equal(200, player.RoundScore);
        Assert.Equal(4, player.TableCards.Count);
        Assert.Empty(player.Hand);
    }

    [Fact]
    public void DeclareMeld_FourKings_ReturnsTrueAndAwardsPoints()
    {
        var king1 = new Card((byte)20, 0);
        var king2 = new Card((byte)21, 1);
        var king3 = new Card((byte)22, 2);
        var king4 = new Card((byte)23, 3);
        var player = new Player(0)
        {
            Hand = new List<Card> { king1, king2, king3, king4 },
            RoundScore = 100
        };

        bool result = MeldStateHandler.DeclareMeld(player, new[] { king1, king2, king3, king4 }, MeldType.FourKings, Suit.Diamonds);

        Assert.True(result);
        Assert.Equal(180, player.RoundScore);
        Assert.Equal(4, player.TableCards.Count);
        Assert.Empty(player.Hand);
    }

    [Fact]
    public void DeclareMeld_FourQueens_ReturnsTrueAndAwardsPoints()
    {
        var queen1 = new Card((byte)16, 0);
        var queen2 = new Card((byte)17, 1);
        var queen3 = new Card((byte)18, 2);
        var queen4 = new Card((byte)19, 3);
        var player = new Player(0)
        {
            Hand = new List<Card> { queen1, queen2, queen3, queen4 },
            RoundScore = 100
        };

        bool result = MeldStateHandler.DeclareMeld(player, new[] { queen1, queen2, queen3, queen4 }, MeldType.FourQueens, Suit.Diamonds);

        Assert.True(result);
        Assert.Equal(160, player.RoundScore);
        Assert.Equal(4, player.TableCards.Count);
        Assert.Empty(player.Hand);
    }

    [Fact]
    public void DeclareMeld_FourJacks_ReturnsTrueAndAwardsPoints()
    {
        var jack1 = new Card((byte)12, 0);
        var jack2 = new Card((byte)13, 1);
        var jack3 = new Card((byte)14, 2);
        var jack4 = new Card((byte)15, 3);
        var player = new Player(0)
        {
            Hand = new List<Card> { jack1, jack2, jack3, jack4 },
            RoundScore = 100
        };

        bool result = MeldStateHandler.DeclareMeld(player, new[] { jack1, jack2, jack3, jack4 }, MeldType.FourJacks, Suit.Diamonds);

        Assert.True(result);
        Assert.Equal(140, player.RoundScore);
        Assert.Equal(4, player.TableCards.Count);
        Assert.Empty(player.Hand);
    }

    [Fact]
    public void DeclareMeld_InvalidMeld_ReturnsFalse()
    {
        var seven = new Card((byte)0, 0);
        var player = new Player(0)
        {
            Hand = new List<Card> { seven },
            RoundScore = 100
        };

        bool result = MeldStateHandler.DeclareMeld(player, new[] { seven }, MeldType.Bezique, Suit.Diamonds);

        Assert.False(result);
        Assert.Equal(100, player.RoundScore);
        Assert.Empty(player.TableCards);
        Assert.Single(player.Hand);
    }

    [Fact]
    public void DeclareMeld_CardNotInHand_ReturnsFalse()
    {
        var card = new Card((byte)0, 0);
        var player = new Player(0)
        {
            Hand = new List<Card>(),
            RoundScore = 100
        };

        bool result = MeldStateHandler.DeclareMeld(player, new[] { card }, MeldType.TrumpSeven, Suit.Diamonds);

        Assert.False(result);
        Assert.Equal(100, player.RoundScore);
        Assert.Empty(player.TableCards);
    }

    [Fact]
    public void DeclareMeld_WithList_ReturnsTrueAndAwardsPoints()
    {
        var king = new Card((byte)20, 0);
        var queen = new Card((byte)16, 0);
        var player = new Player(0)
        {
            Hand = new List<Card> { king, queen },
            RoundScore = 100
        };

        bool result = MeldStateHandler.DeclareMeld(player, new[] { king, queen }, MeldType.TrumpMarriage, Suit.Diamonds);

        Assert.True(result);
        Assert.Equal(140, player.RoundScore);
        Assert.Equal(2, player.TableCards.Count);
        Assert.Empty(player.Hand);
    }

    [Fact]
    public void DeclareMeld_CardAlreadyUsed_ReturnsFalse()
    {
        var king = new Card((byte)20, 0);
        var queen = new Card((byte)16, 0);
        var player = new Player(0)
        {
            Hand = new List<Card> { king, queen },
            RoundScore = 100,
            TableCards = new List<Card>()
        };
        player.MeldHistory[MeldType.TrumpMarriage] = new List<Card>
        {
            new Card((byte)20, 0),
            new Card((byte)16, 0)
        };

        bool result = MeldStateHandler.DeclareMeld(player, new[] { king, queen }, MeldType.TrumpMarriage, Suit.Diamonds);

        Assert.False(result);
        Assert.Equal(100, player.RoundScore);
    }

    [Fact]
    public void DeclareMeld_SameCardDifferentMeld_ReturnsTrue()
    {
        var king = new Card((byte)20, 0);
        var queen = new Card((byte)16, 0);
        var player = new Player(0)
        {
            Hand = new List<Card> { king, queen },
            RoundScore = 100
        };
        player.MeldHistory[MeldType.TrumpRun] = new List<Card>
        {
            new Card((byte)20, 0)
        };

        bool result = MeldStateHandler.DeclareMeld(player, new[] { king, queen }, MeldType.TrumpMarriage, Suit.Diamonds);

        Assert.True(result);
        Assert.Equal(140, player.RoundScore);
    }
}
