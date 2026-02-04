using BeziqueCore;

public class MeldValidatorTests
{
    [Fact]
    public void GetPoints_ReturnsCorrectValue_ForAllMeldTypes()
    {
        Assert.Equal(10, MeldValidator.GetPoints(MeldType.TrumpSeven));
        Assert.Equal(40, MeldValidator.GetPoints(MeldType.TrumpMarriage));
        Assert.Equal(20, MeldValidator.GetPoints(MeldType.NonTrumpMarriage));
        Assert.Equal(40, MeldValidator.GetPoints(MeldType.Bezique));
        Assert.Equal(500, MeldValidator.GetPoints(MeldType.DoubleBezique));
        Assert.Equal(250, MeldValidator.GetPoints(MeldType.TrumpRun));
        Assert.Equal(100, MeldValidator.GetPoints(MeldType.FourAces));
        Assert.Equal(80, MeldValidator.GetPoints(MeldType.FourKings));
        Assert.Equal(60, MeldValidator.GetPoints(MeldType.FourQueens));
        Assert.Equal(40, MeldValidator.GetPoints(MeldType.FourJacks));
    }

    [Fact]
    public void GetRequiredCardCount_ReturnsCorrectValue_ForAllMeldTypes()
    {
        Assert.Equal(1, MeldValidator.GetRequiredCardCount(MeldType.TrumpSeven));
        Assert.Equal(2, MeldValidator.GetRequiredCardCount(MeldType.TrumpMarriage));
        Assert.Equal(2, MeldValidator.GetRequiredCardCount(MeldType.NonTrumpMarriage));
        Assert.Equal(2, MeldValidator.GetRequiredCardCount(MeldType.Bezique));
        Assert.Equal(4, MeldValidator.GetRequiredCardCount(MeldType.DoubleBezique));
        Assert.Equal(5, MeldValidator.GetRequiredCardCount(MeldType.TrumpRun));
        Assert.Equal(4, MeldValidator.GetRequiredCardCount(MeldType.FourAces));
        Assert.Equal(4, MeldValidator.GetRequiredCardCount(MeldType.FourKings));
        Assert.Equal(4, MeldValidator.GetRequiredCardCount(MeldType.FourQueens));
        Assert.Equal(4, MeldValidator.GetRequiredCardCount(MeldType.FourJacks));
    }

    [Fact]
    public void GetPoints_ReturnsZero_ForInvalidMeldType()
    {
        var invalidMeld = (MeldType)99;
        Assert.Equal(0, MeldValidator.GetPoints(invalidMeld));
    }

    [Fact]
    public void GetRequiredCardCount_ReturnsZero_ForInvalidMeldType()
    {
        var invalidMeld = (MeldType)99;
        Assert.Equal(0, MeldValidator.GetRequiredCardCount(invalidMeld));
    }

    [Fact]
    public void ValidateMeld_TrumpSeven_ReturnsTrue_WhenValid()
    {
        var cards = new[] { new Card(3, 0) };
        var result = MeldValidator.ValidateMeld(MeldType.TrumpSeven, cards, Suit.Spades);
        Assert.True(result);
    }

    [Fact]
    public void ValidateMeld_TrumpSeven_ReturnsFalse_WhenWrongSuit()
    {
        var cards = new[] { new Card(0, 0) };
        var result = MeldValidator.ValidateMeld(MeldType.TrumpSeven, cards, Suit.Spades);
        Assert.False(result);
    }

    [Fact]
    public void ValidateMeld_TrumpMarriage_ReturnsTrue_WhenValid()
    {
        var cards = new[] { new Card(22, 0), new Card(18, 0) };
        var result = MeldValidator.ValidateMeld(MeldType.TrumpMarriage, cards, Suit.Hearts);
        Assert.True(result);
    }

    [Fact]
    public void ValidateMeld_NonTrumpMarriage_ReturnsTrue_WhenValid()
    {
        var cards = new[] { new Card(22, 0), new Card(18, 0) };
        var result = MeldValidator.ValidateMeld(MeldType.NonTrumpMarriage, cards, Suit.Spades);
        Assert.True(result);
    }

    [Fact]
    public void ValidateMeld_NonTrumpMarriage_ReturnsFalse_WhenDifferentSuits()
    {
        var cards = new[] { new Card(20, 0), new Card(19, 0) };
        var result = MeldValidator.ValidateMeld(MeldType.NonTrumpMarriage, cards, Suit.Hearts);
        Assert.False(result);
    }

    [Fact]
    public void ValidateMeld_Bezique_ReturnsTrue_WhenValid()
    {
        var cards = new[] { new Card(19, 0), new Card(12, 0) };
        var result = MeldValidator.ValidateMeld(MeldType.Bezique, cards, Suit.Hearts);
        Assert.True(result);
    }

    [Fact]
    public void ValidateMeld_Bezique_ReturnsFalse_WhenWrongCards()
    {
        var cards = new[] { new Card(19, 0), new Card(14, 0) };
        var result = MeldValidator.ValidateMeld(MeldType.Bezique, cards, Suit.Hearts);
        Assert.False(result);
    }

    [Fact]
    public void ValidateMeld_DoubleBezique_ReturnsTrue_WhenValid()
    {
        var cards = new[] { new Card(19, 0), new Card(19, 1), new Card(12, 0), new Card(12, 1) };
        var result = MeldValidator.ValidateMeld(MeldType.DoubleBezique, cards, Suit.Diamonds);
        Assert.True(result);
    }

    [Fact]
    public void ValidateMeld_DoubleBezique_ReturnsFalse_WhenNotEnoughCards()
    {
        var cards = new[] { new Card(19, 0), new Card(19, 1), new Card(12, 0) };
        var result = MeldValidator.ValidateMeld(MeldType.DoubleBezique, cards, Suit.Diamonds);
        Assert.False(result);
    }

    [Fact]
    public void ValidateMeld_TrumpRun_ReturnsTrue_WhenValid()
    {
        var cards = new[] { new Card(28, 0), new Card(24, 0), new Card(20, 0), new Card(16, 0), new Card(12, 0) };
        var result = MeldValidator.ValidateMeld(MeldType.TrumpRun, cards, Suit.Diamonds);
        Assert.True(result);
    }

    [Fact]
    public void ValidateMeld_TrumpRun_ReturnsFalse_WhenMissingCard()
    {
        var cards = new[] { new Card(28, 0), new Card(24, 0), new Card(20, 0), new Card(16, 0) };
        var result = MeldValidator.ValidateMeld(MeldType.TrumpRun, cards, Suit.Diamonds);
        Assert.False(result);
    }

    [Fact]
    public void ValidateMeld_FourAces_ReturnsTrue_WhenValid()
    {
        var cards = new[] { new Card(28, 0), new Card(29, 0), new Card(30, 0), new Card(31, 0) };
        var result = MeldValidator.ValidateMeld(MeldType.FourAces, cards, Suit.Diamonds);
        Assert.True(result);
    }

    [Fact]
    public void ValidateMeld_FourKings_ReturnsFalse_WhenNotAllKings()
    {
        var cards = new[] { new Card(20, 0), new Card(21, 0), new Card(22, 0), new Card(23, 0) };
        var result = MeldValidator.ValidateMeld(MeldType.FourAces, cards, Suit.Diamonds);
        Assert.False(result);
    }

    [Fact]
    public void HasCardBeenUsedForMeld_ReturnsFalse_WhenNoHistory()
    {
        var player = new Player(0);
        var card = new Card(10, 0);

        var result = MeldValidator.HasCardBeenUsedForMeld(player, card, MeldType.FourAces);

        Assert.False(result);
    }

    [Fact]
    public void HasCardBeenUsedForMeld_ReturnsFalse_WhenDifferentMeldType()
    {
        var player = new Player(0);
        var card = new Card(10, 0);
        player.MeldHistory[MeldType.FourKings] = new List<Card> { card };

        var result = MeldValidator.HasCardBeenUsedForMeld(player, card, MeldType.FourAces);

        Assert.False(result);
    }

    [Fact]
    public void HasCardBeenUsedForMeld_ReturnsTrue_WhenSameMeldType()
    {
        var player = new Player(0);
        var card = new Card(10, 0);
        player.MeldHistory[MeldType.FourAces] = new List<Card> { card };

        var result = MeldValidator.HasCardBeenUsedForMeld(player, card, MeldType.FourAces);

        Assert.True(result);
    }

    [Fact]
    public void HasCardBeenUsedForMeld_ReturnsFalse_WhenDifferentCardId()
    {
        var player = new Player(0);
        var card1 = new Card(10, 0);
        var card2 = new Card(20, 0);
        player.MeldHistory[MeldType.FourAces] = new List<Card> { card1 };

        var result = MeldValidator.HasCardBeenUsedForMeld(player, card2, MeldType.FourAces);

        Assert.False(result);
    }

    [Fact]
    public void CanUseCardsForMeld_ReturnsTrue_WhenNoCardsUsed()
    {
        var player = new Player(0);
        var cards = new[] { new Card(10, 0), new Card(20, 0) };

        var result = MeldValidator.CanUseCardsForMeld(player, cards, MeldType.FourAces);

        Assert.True(result);
    }

    [Fact]
    public void CanUseCardsForMeld_ReturnsFalse_WhenAnyCardUsed()
    {
        var player = new Player(0);
        var card1 = new Card(10, 0);
        var card2 = new Card(20, 0);
        player.MeldHistory[MeldType.FourAces] = new List<Card> { card1 };
        var cards = new[] { card1, card2 };

        var result = MeldValidator.CanUseCardsForMeld(player, cards, MeldType.FourAces);

        Assert.False(result);
    }

    [Fact]
    public void CanUseCardsForMeld_ReturnsTrue_WhenCardsUsedInDifferentMeld()
    {
        var player = new Player(0);
        var card1 = new Card(10, 0);
        var card2 = new Card(20, 0);
        player.MeldHistory[MeldType.FourKings] = new List<Card> { card1, card2 };
        var cards = new[] { card1, card2 };

        var result = MeldValidator.CanUseCardsForMeld(player, cards, MeldType.FourAces);

        Assert.True(result);
    }

    [Fact]
    public void TryExecuteMeld_ReturnsTrue_WhenValidTrumpSeven()
    {
        var player = new Player(0);
        player.Hand.Add(new Card(3, 0));
        var cards = new[] { player.Hand[0] };

        var result = MeldValidator.TryExecuteMeld(player, cards, MeldType.TrumpSeven, Suit.Spades);

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

        var result = MeldValidator.TryExecuteMeld(player, cards, MeldType.TrumpMarriage, Suit.Hearts);

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

        var result = MeldValidator.TryExecuteMeld(player, cards, MeldType.FourAces, Suit.Diamonds);

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

        var result = MeldValidator.TryExecuteMeld(player, cards, MeldType.FourAces, Suit.Diamonds);

        Assert.False(result);
    }

    [Fact]
    public void TryExecuteMeld_UpdatesMeldHistory()
    {
        var player = new Player(0);
        player.Hand.Add(new Card(3, 0));
        var cards = new[] { player.Hand[0] };

        MeldValidator.TryExecuteMeld(player, cards, MeldType.TrumpSeven, Suit.Spades);

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

        var result = MeldValidator.TryExecuteMeld(player, cards, MeldType.NonTrumpMarriage, Suit.Spades);

        Assert.True(result);
    }

    [Fact]
    public void TryExecuteMeld_AwardsCorrectPoints_ForAllMeldTypes()
    {
        var player = new Player(0);

        Assert.Equal(10, MeldValidator.GetPoints(MeldType.TrumpSeven));
        Assert.Equal(40, MeldValidator.GetPoints(MeldType.TrumpMarriage));
        Assert.Equal(20, MeldValidator.GetPoints(MeldType.NonTrumpMarriage));
        Assert.Equal(40, MeldValidator.GetPoints(MeldType.Bezique));
        Assert.Equal(500, MeldValidator.GetPoints(MeldType.DoubleBezique));
        Assert.Equal(250, MeldValidator.GetPoints(MeldType.TrumpRun));
        Assert.Equal(100, MeldValidator.GetPoints(MeldType.FourAces));
        Assert.Equal(80, MeldValidator.GetPoints(MeldType.FourKings));
        Assert.Equal(60, MeldValidator.GetPoints(MeldType.FourQueens));
        Assert.Equal(40, MeldValidator.GetPoints(MeldType.FourJacks));
    }

    [Fact]
    public void TryExecuteMeld_WithList_ReturnsTrue_WhenValid()
    {
        var player = new Player(0);
        var cards = new List<Card> { new Card(3, 0) };
        player.Hand.Add(cards[0]);

        var result = MeldValidator.TryExecuteMeld(player, cards, MeldType.TrumpSeven, Suit.Spades);

        Assert.True(result);
        Assert.Equal(10, player.RoundScore);
    }

    [Fact]
    public void TryExecuteMeld_WithList_ReturnsFalse_WhenInvalid()
    {
        var player = new Player(0);
        var cards = new List<Card> { new Card(10, 0) };
        player.Hand.Add(cards[0]);

        var result = MeldValidator.TryExecuteMeld(player, cards, MeldType.FourAces, Suit.Diamonds);

        Assert.False(result);
    }
}
