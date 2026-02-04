using BeziqueCore;

public class MeldReuseCheckerTests
{
    [Fact]
    public void HasCardBeenUsedForMeld_ReturnsFalse_WhenNoHistory()
    {
        var player = new Player(0);
        var card = new Card(10, 0);

        var result = MeldReuseChecker.HasCardBeenUsedForMeld(player, card, MeldType.FourAces);

        Assert.False(result);
    }

    [Fact]
    public void HasCardBeenUsedForMeld_ReturnsFalse_WhenDifferentMeldType()
    {
        var player = new Player(0);
        var card = new Card(10, 0);
        player.MeldHistory[MeldType.FourKings] = new List<Card> { card };

        var result = MeldReuseChecker.HasCardBeenUsedForMeld(player, card, MeldType.FourAces);

        Assert.False(result);
    }

    [Fact]
    public void HasCardBeenUsedForMeld_ReturnsTrue_WhenSameMeldType()
    {
        var player = new Player(0);
        var card = new Card(10, 0);
        player.MeldHistory[MeldType.FourAces] = new List<Card> { card };

        var result = MeldReuseChecker.HasCardBeenUsedForMeld(player, card, MeldType.FourAces);

        Assert.True(result);
    }

    [Fact]
    public void HasCardBeenUsedForMeld_ReturnsFalse_WhenDifferentCardId()
    {
        var player = new Player(0);
        var card1 = new Card(10, 0);
        var card2 = new Card(20, 0);
        player.MeldHistory[MeldType.FourAces] = new List<Card> { card1 };

        var result = MeldReuseChecker.HasCardBeenUsedForMeld(player, card2, MeldType.FourAces);

        Assert.False(result);
    }

    [Fact]
    public void CanUseCardsForMeld_ReturnsTrue_WhenNoCardsUsed()
    {
        var player = new Player(0);
        var cards = new[] { new Card(10, 0), new Card(20, 0) };

        var result = MeldReuseChecker.CanUseCardsForMeld(player, cards, MeldType.FourAces);

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

        var result = MeldReuseChecker.CanUseCardsForMeld(player, cards, MeldType.FourAces);

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

        var result = MeldReuseChecker.CanUseCardsForMeld(player, cards, MeldType.FourAces);

        Assert.True(result);
    }

    [Fact]
    public void AnyCardsAlreadyUsed_ReturnsTrue_WhenCardUsed()
    {
        var player = new Player(0);
        var card1 = new Card(10, 0);
        var card2 = new Card(20, 0);
        player.MeldHistory[MeldType.FourAces] = new List<Card> { card1 };
        var cards = new[] { card1, card2 };

        var result = MeldReuseChecker.AnyCardsAlreadyUsed(player, cards, MeldType.FourAces);

        Assert.True(result);
    }

    [Fact]
    public void AnyCardsAlreadyUsed_ReturnsFalse_WhenNoCardsUsed()
    {
        var player = new Player(0);
        var cards = new[] { new Card(10, 0), new Card(20, 0) };

        var result = MeldReuseChecker.AnyCardsAlreadyUsed(player, cards, MeldType.FourAces);

        Assert.False(result);
    }
}
