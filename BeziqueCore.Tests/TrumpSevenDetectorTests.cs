using BeziqueCore;

public class TrumpSevenDetectorTests
{
    [Fact]
    public void IsTrumpSeven_ReturnsTrue_WhenCardIsTrumpSeven()
    {
        var sevenOfSpades = new Card(3, 0);
        var result = TrumpSevenDetector.IsTrumpSeven(sevenOfSpades, Suit.Spades);
        Assert.True(result);
    }

    [Fact]
    public void IsTrumpSeven_ReturnsFalse_WhenCardIsNotSeven()
    {
        var aceOfSpades = new Card(31, 0);
        var result = TrumpSevenDetector.IsTrumpSeven(aceOfSpades, Suit.Spades);
        Assert.False(result);
    }

    [Fact]
    public void IsTrumpSeven_ReturnsFalse_WhenCardIsNotTrump()
    {
        var sevenOfHearts = new Card(2, 0);
        var result = TrumpSevenDetector.IsTrumpSeven(sevenOfHearts, Suit.Spades);
        Assert.False(result);
    }

    [Fact]
    public void IsTrumpSeven_ReturnsFalse_WhenCardIsJoker()
    {
        var joker = new Card(32, 0);
        var result = TrumpSevenDetector.IsTrumpSeven(joker, Suit.Spades);
        Assert.False(result);
    }

    [Fact]
    public void FindTrumpSevenPlayers_ReturnsEmptyArray_WhenNoTrumpSeven()
    {
        var playedCards = new[] { new Card(10, 0), new Card(20, 0) };
        var result = TrumpSevenDetector.FindTrumpSevenPlayers(playedCards, Suit.Spades);
        Assert.Empty(result);
    }

    [Fact]
    public void FindTrumpSevenPlayers_ReturnsPlayerIndex_WhenOneTrumpSeven()
    {
        var playedCards = new[] { new Card(3, 0), new Card(20, 0) };
        var result = TrumpSevenDetector.FindTrumpSevenPlayers(playedCards, Suit.Spades);
        Assert.Single(result);
        Assert.Contains(0, result);
    }

    [Fact]
    public void FindTrumpSevenPlayers_ReturnsMultiplePlayerIndexes_WhenMultipleTrumpSevens()
    {
        var playedCards = new[] { new Card(3, 0), new Card(3, 1) };
        var result = TrumpSevenDetector.FindTrumpSevenPlayers(playedCards, Suit.Spades);
        Assert.Equal(2, result.Length);
        Assert.Contains(0, result);
        Assert.Contains(1, result);
    }
}
