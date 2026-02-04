using BeziqueCore;

public class MeldRuleValidatorTests
{
    [Fact]
    public void ValidateMeld_TrumpSeven_ReturnsTrue_WhenValid()
    {
        var cards = new[] { new Card(3, 0) };
        var result = MeldRuleValidator.ValidateMeld(MeldType.TrumpSeven, cards, Suit.Spades);
        Assert.True(result);
    }

    [Fact]
    public void ValidateMeld_TrumpSeven_ReturnsFalse_WhenWrongSuit()
    {
        var cards = new[] { new Card(0, 0) };
        var result = MeldRuleValidator.ValidateMeld(MeldType.TrumpSeven, cards, Suit.Spades);
        Assert.False(result);
    }

    [Fact]
    public void ValidateMeld_TrumpMarriage_ReturnsTrue_WhenValid()
    {
        var cards = new[] { new Card(22, 0), new Card(18, 0) };
        var result = MeldRuleValidator.ValidateMeld(MeldType.TrumpMarriage, cards, Suit.Hearts);
        Assert.True(result);
    }

    [Fact]
    public void ValidateMeld_NonTrumpMarriage_ReturnsTrue_WhenValid()
    {
        var cards = new[] { new Card(20, 0), new Card(18, 0) };
        var result = MeldRuleValidator.ValidateMeld(MeldType.NonTrumpMarriage, cards, Suit.Hearts);
        Assert.True(result);
    }

    [Fact]
    public void ValidateMeld_NonTrumpMarriage_ReturnsFalse_WhenDifferentSuits()
    {
        var cards = new[] { new Card(20, 0), new Card(19, 0) };
        var result = MeldRuleValidator.ValidateMeld(MeldType.NonTrumpMarriage, cards, Suit.Hearts);
        Assert.False(result);
    }

    [Fact]
    public void ValidateMeld_Bezique_ReturnsTrue_WhenValid()
    {
        var cards = new[] { new Card(15, 0), new Card(12, 0) };
        var result = MeldRuleValidator.ValidateMeld(MeldType.Bezique, cards, Suit.Clubs);
        Assert.True(result);
    }

    [Fact]
    public void ValidateMeld_Bezique_ReturnsFalse_WhenWrongCards()
    {
        var cards = new[] { new Card(15, 0), new Card(14, 0) };
        var result = MeldRuleValidator.ValidateMeld(MeldType.Bezique, cards, Suit.Clubs);
        Assert.False(result);
    }

    [Fact]
    public void ValidateMeld_DoubleBezique_ReturnsTrue_WhenValid()
    {
        var cards = new[] { new Card(15, 0), new Card(15, 1), new Card(12, 0), new Card(12, 1) };
        var result = MeldRuleValidator.ValidateMeld(MeldType.DoubleBezique, cards, Suit.Clubs);
        Assert.True(result);
    }

    [Fact]
    public void ValidateMeld_DoubleBezique_ReturnsFalse_WhenNotEnoughCards()
    {
        var cards = new[] { new Card(15, 0), new Card(15, 1), new Card(12, 0) };
        var result = MeldRuleValidator.ValidateMeld(MeldType.DoubleBezique, cards, Suit.Clubs);
        Assert.False(result);
    }

    [Fact]
    public void ValidateMeld_TrumpRun_ReturnsTrue_WhenValid()
    {
        var cards = new[] { new Card(30, 0), new Card(25, 0), new Card(22, 0), new Card(18, 0), new Card(13, 0) };
        var result = MeldRuleValidator.ValidateMeld(MeldType.TrumpRun, cards, Suit.Diamonds);
        Assert.True(result);
    }

    [Fact]
    public void ValidateMeld_TrumpRun_ReturnsFalse_WhenMissingCard()
    {
        var cards = new[] { new Card(30, 0), new Card(25, 0), new Card(22, 0), new Card(18, 0) };
        var result = MeldRuleValidator.ValidateMeld(MeldType.TrumpRun, cards, Suit.Diamonds);
        Assert.False(result);
    }

    [Fact]
    public void ValidateMeld_FourAces_ReturnsTrue_WhenValid()
    {
        var cards = new[] { new Card(28, 0), new Card(29, 0), new Card(30, 0), new Card(31, 0) };
        var result = MeldRuleValidator.ValidateMeld(MeldType.FourAces, cards, Suit.Diamonds);
        Assert.True(result);
    }

    [Fact]
    public void ValidateMeld_FourKings_ReturnsFalse_WhenNotAllKings()
    {
        var cards = new[] { new Card(20, 0), new Card(21, 0), new Card(22, 0), new Card(23, 0) };
        var result = MeldRuleValidator.ValidateMeld(MeldType.FourAces, cards, Suit.Diamonds);
        Assert.False(result);
    }
}
