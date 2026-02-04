using BeziqueCore;

public class TrickEvaluatorTests
{
    [Fact]
    public void GetWinner_JokerVsNonTrump_JokerWins()
    {
        var joker = DeckFactory.CreateJoker(0);
        var tenOfDiamonds = new Card(25, 0);
        var played = new[] { joker, tenOfDiamonds };
        var result = TrickEvaluator.GetWinner(played, Suit.Hearts);
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetWinner_JokerVsTrump_TrumpWins()
    {
        var joker = DeckFactory.CreateJoker(0);
        var queenOfHearts = new Card(18, 0);
        var played = new[] { joker, queenOfHearts };
        var result = TrickEvaluator.GetWinner(played, Suit.Hearts);
        Assert.Equal(1, result);
    }

    [Fact]
    public void GetWinner_TenVsKing_TenWins()
    {
        var tenOfDiamonds = new Card(25, 0);
        var kingOfDiamonds = new Card(27, 0);
        var played = new[] { tenOfDiamonds, kingOfDiamonds };
        var result = TrickEvaluator.GetWinner(played, Suit.Hearts);
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetWinner_TrumpVsNonTrump_TrumpWins()
    {
        var aceOfDiamonds = new Card(28, 0);
        var sevenOfSpades = new Card(3, 0);
        var played = new[] { aceOfDiamonds, sevenOfSpades };
        var result = TrickEvaluator.GetWinner(played, Suit.Spades);
        Assert.Equal(1, result);
    }

    [Fact]
    public void GetWinner_SameSuit_HigherRankWins()
    {
        var queenOfHearts = new Card(18, 0);
        var aceOfHearts = new Card(30, 0);
        var played = new[] { queenOfHearts, aceOfHearts };
        var result = TrickEvaluator.GetWinner(played, Suit.Diamonds);
        Assert.Equal(1, result);
    }

    [Fact]
    public void GetWinner_LeadSuitVsOffSuit_LeadSuitWins()
    {
        var jackOfClubs = new Card(13, 0);
        var kingOfDiamonds = new Card(20, 0);
        var played = new[] { jackOfClubs, kingOfDiamonds };
        var result = TrickEvaluator.GetWinner(played, Suit.Hearts);

        Assert.Equal(0, result);
    }

    [Fact]
    public void GetWinner_FollowJoker_AlwaysLoses()
    {
        var aceOfSpades = new Card(31, 0);
        var joker = DeckFactory.CreateJoker(1);
        var played = new[] { aceOfSpades, joker };
        var result = TrickEvaluator.GetWinner(played, Suit.Diamonds);
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetWinner_TrumpVsLeadSuit_TrumpWins()
    {
        var kingOfHearts = new Card(22, 0);
        var sevenOfSpades = new Card(3, 0);
        var played = new[] { kingOfHearts, sevenOfSpades };
        var result = TrickEvaluator.GetWinner(played, Suit.Spades);
        Assert.Equal(1, result);
    }

    [Fact]
    public void GetWinner_HigherTrumpBeatsLowerTrump()
    {
        var sevenOfSpades = new Card(3, 0);
        var aceOfSpades = new Card(31, 0);
        var played = new[] { sevenOfSpades, aceOfSpades };
        var result = TrickEvaluator.GetWinner(played, Suit.Spades);
        Assert.Equal(1, result);
    }

    [Fact]
    public void GetWinner_EmptyArray_ReturnsNegativeOne()
    {
        var played = Array.Empty<Card>();
        var result = TrickEvaluator.GetWinner(played, Suit.Hearts);
        Assert.Equal(-1, result);
    }

    [Fact]
    public void GetWinner_FourPlayers_CorrectWinner()
    {
        var jackOfClubs = new Card(13, 0);
        var queenOfDiamonds = new Card(19, 0);
        var kingOfHearts = new Card(22, 0);
        var aceOfSpades = new Card(31, 0);
        var played = new[] { jackOfClubs, queenOfDiamonds, kingOfHearts, aceOfSpades };
        var result = TrickEvaluator.GetWinner(played, Suit.Spades);
        Assert.Equal(3, result);
    }

    [Fact]
    public void GetWinner_MultipleTrumps_HighestTrumpWins()
    {
        var tenOfSpades = new Card(25, 0);
        var aceOfSpades = new Card(31, 0);
        var sevenOfSpades = new Card(3, 0);
        var played = new[] { tenOfSpades, aceOfSpades, sevenOfSpades };
        var result = TrickEvaluator.GetWinner(played, Suit.Spades);
        Assert.Equal(1, result);
    }
}
