using BeziqueCore;

public class PlayCardValidatorTests
{
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

    [Fact]
    public void TryRemoveCard_ReturnsTrue_WhenCardExists()
    {
        var cards = new List<Card> { new Card(5, 0), new Card(10, 1), new Card(15, 2) };
        var target = new Card(10, 1);

        var result = PlayCardValidator.TryRemoveCard(cards, target);

        Assert.True(result);
        Assert.Equal(2, cards.Count);
        Assert.DoesNotContain(target, cards);
    }

    [Fact]
    public void TryRemoveCard_ReturnsFalse_WhenCardNotExists()
    {
        var cards = new List<Card> { new Card(5, 0), new Card(10, 1), new Card(15, 2) };
        var target = new Card(20, 0);

        var result = PlayCardValidator.TryRemoveCard(cards, target);

        Assert.False(result);
        Assert.Equal(3, cards.Count);
    }
}
