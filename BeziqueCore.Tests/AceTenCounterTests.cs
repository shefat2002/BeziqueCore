using BeziqueCore;

public class AceTenCounterTests
{
    [Fact]
    public void CountAcesAndTens_EmptyPile_ReturnsZero()
    {
        var wonPile = new List<Card>();
        Assert.Equal(0, AceTenCounter.CountAcesAndTens(wonPile));
    }

    [Fact]
    public void CountAcesAndTens_NoAcesOrTens_ReturnsZero()
    {
        var wonPile = new List<Card>
        {
            new Card((byte)0, 0),
            new Card((byte)4, 0),
            new Card((byte)8, 0)
        };
        Assert.Equal(0, AceTenCounter.CountAcesAndTens(wonPile));
    }

    [Fact]
    public void CountAcesAndTens_OnlyAces_ReturnsAceCount()
    {
        var wonPile = new List<Card>
        {
            new Card((byte)28, 0),
            new Card((byte)29, 0),
            new Card((byte)30, 0)
        };
        Assert.Equal(3, AceTenCounter.CountAcesAndTens(wonPile));
    }

    [Fact]
    public void CountAcesAndTens_OnlyTens_ReturnsTenCount()
    {
        var wonPile = new List<Card>
        {
            new Card((byte)24, 0),
            new Card((byte)25, 0),
            new Card((byte)26, 0),
            new Card((byte)27, 0)
        };
        Assert.Equal(4, AceTenCounter.CountAcesAndTens(wonPile));
    }

    [Fact]
    public void CountAcesAndTens_MixedCards_ReturnsCorrectCount()
    {
        var wonPile = new List<Card>
        {
            new Card((byte)0, 0),
            new Card((byte)24, 0),
            new Card((byte)4, 0),
            new Card((byte)28, 0),
            new Card((byte)8, 0),
            new Card((byte)25, 0)
        };
        Assert.Equal(3, AceTenCounter.CountAcesAndTens(wonPile));
    }

    [Fact]
    public void CountAcesAndTens_AllAcesAndTens_ReturnsTotalCount()
    {
        var wonPile = new List<Card>();
        for (sbyte i = 0; i < 4; i++)
        {
            wonPile.Add(new Card((byte)28, i));
            wonPile.Add(new Card((byte)24, i));
        }
        Assert.Equal(8, AceTenCounter.CountAcesAndTens(wonPile));
    }

    [Fact]
    public void CountAcesAndTens_IgnoresJokers()
    {
        var wonPile = new List<Card>
        {
            new Card((byte)28, 0),
            new Card((byte)32, 0),
            new Card((byte)24, 0)
        };
        Assert.Equal(2, AceTenCounter.CountAcesAndTens(wonPile));
    }
}
