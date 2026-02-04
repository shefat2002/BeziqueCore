using BeziqueCore;

public class PlayCardValidatorTests
{
    [Fact]
    public void CanPlayCard_ReturnsTrue_WhenCardInHand()
    {
        var player = new Player(0);
        var card = new Card(10, 0);
        player.Hand.Add(card);

        var result = PlayCardValidator.CanPlayCard(player, card);

        Assert.True(result);
    }

    [Fact]
    public void CanPlayCard_ReturnsTrue_WhenCardInTableCards()
    {
        var player = new Player(0);
        var card = new Card(10, 0);
        player.TableCards.Add(card);

        var result = PlayCardValidator.CanPlayCard(player, card);

        Assert.True(result);
    }

    [Fact]
    public void CanPlayCard_ReturnsFalse_WhenCardNotOwned()
    {
        var player = new Player(0);
        var card = new Card(10, 0);

        var result = PlayCardValidator.CanPlayCard(player, card);

        Assert.False(result);
    }

    [Fact]
    public void TryPlayCard_RemovesFromHand_WhenCardInHand()
    {
        var player = new Player(0);
        var card = new Card(10, 0);
        player.Hand.Add(card);

        var result = PlayCardValidator.TryPlayCard(player, card, out var playedCard);

        Assert.True(result);
        Assert.Equal(card, playedCard);
        Assert.Empty(player.Hand);
    }

    [Fact]
    public void TryPlayCard_RemovesFromTableCards_WhenCardInTableCards()
    {
        var player = new Player(0);
        var card = new Card(10, 0);
        player.TableCards.Add(card);

        var result = PlayCardValidator.TryPlayCard(player, card, out var playedCard);

        Assert.True(result);
        Assert.Equal(card, playedCard);
        Assert.Empty(player.TableCards);
    }

    [Fact]
    public void TryPlayCard_ReturnsFalse_WhenCardNotOwned()
    {
        var player = new Player(0);
        var card = new Card(10, 0);

        var result = PlayCardValidator.TryPlayCard(player, card, out var playedCard);

        Assert.False(result);
        Assert.Equal(default, playedCard);
    }

    [Fact]
    public void TryPlayCard_PrefersHand_WhenCardInBoth()
    {
        var player = new Player(0);
        var card = new Card(10, 0);
        player.Hand.Add(card);
        player.TableCards.Add(card);

        var result = PlayCardValidator.TryPlayCard(player, card, out var playedCard);

        Assert.True(result);
        Assert.Empty(player.Hand);
        Assert.Single(player.TableCards);
    }

    [Fact]
    public void ContainsCard_ReturnsTrue_WhenCardExists()
    {
        var cards = new List<Card> { new Card(5, 0), new Card(10, 1), new Card(15, 2) };
        var target = new Card(10, 1);

        var result = PlayCardValidator.ContainsCard(cards, target);

        Assert.True(result);
    }

    [Fact]
    public void ContainsCard_ReturnsFalse_WhenCardNotExists()
    {
        var cards = new List<Card> { new Card(5, 0), new Card(10, 1), new Card(15, 2) };
        var target = new Card(20, 0);

        var result = PlayCardValidator.ContainsCard(cards, target);

        Assert.False(result);
    }

    [Fact]
    public void ContainsCard_ReturnsFalse_WhenDifferentDeckIndex()
    {
        var cards = new List<Card> { new Card(10, 0) };
        var target = new Card(10, 1);

        var result = PlayCardValidator.ContainsCard(cards, target);

        Assert.False(result);
    }
}
