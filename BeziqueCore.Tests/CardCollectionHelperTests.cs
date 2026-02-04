using BeziqueCore;

public class CardCollectionHelperTests
{
    [Fact]
    public void CombineHandAndTable_ReturnsAllCards()
    {
        var player = new Player(0);
        player.Hand.Add(new Card(10, 0));
        player.Hand.Add(new Card(20, 0));
        player.TableCards.Add(new Card(30, 0));

        var combined = CardCollectionHelper.CombineHandAndTable(player);

        Assert.Equal(3, combined.Count);
    }

    [Fact]
    public void TryFindCard_ReturnsTrue_WhenCardExists()
    {
        var cards = new List<Card> { new Card(10, 0), new Card(20, 1) };
        var result = CardCollectionHelper.TryFindCard(cards, 10, out var found);

        Assert.True(result);
        Assert.Equal(10, found.CardId);
    }

    [Fact]
    public void TryFindCard_ReturnsFalse_WhenCardNotExists()
    {
        var cards = new List<Card> { new Card(10, 0), new Card(20, 1) };
        var result = CardCollectionHelper.TryFindCard(cards, 30, out var found);

        Assert.False(result);
    }

    [Fact]
    public void TryFindCards_ReturnsTrue_WhenAllCardsExist()
    {
        var cards = new List<Card> { new Card(10, 0), new Card(20, 1), new Card(30, 2) };
        var cardIds = new byte[] { 10, 20 };

        var result = CardCollectionHelper.TryFindCards(cards, cardIds, out var found);

        Assert.True(result);
        Assert.Equal(2, found.Count);
    }

    [Fact]
    public void TryFindCards_ReturnsFalse_WhenAnyCardMissing()
    {
        var cards = new List<Card> { new Card(10, 0), new Card(20, 1) };
        var cardIds = new byte[] { 10, 30 };

        var result = CardCollectionHelper.TryFindCards(cards, cardIds, out var found);

        Assert.False(result);
    }

    [Fact]
    public void FindCardsBySuit_ReturnsMatchingCards()
    {
        var cards = new List<Card>
        {
            new Card(0, 0), new Card(1, 0), new Card(3, 0), new Card(7, 0)
        };

        var spades = CardCollectionHelper.FindCardsBySuit(cards, Suit.Spades);

        Assert.Equal(2, spades.Count);
    }

    [Fact]
    public void FindCardsByRank_ReturnsMatchingCards()
    {
        var cards = new List<Card>
        {
            new Card(0, 0), new Card(1, 0), new Card(2, 0), new Card(3, 0)
        };

        var sevens = CardCollectionHelper.FindCardsByRank(cards, Rank.Seven);

        Assert.Equal(4, sevens.Count);
    }

    [Fact]
    public void HasCard_ReturnsTrue_WhenCardExists()
    {
        var cards = new List<Card> { new Card(10, 0), new Card(20, 1) };

        var result = CardCollectionHelper.HasCard(cards, 10);

        Assert.True(result);
    }

    [Fact]
    public void HasCard_ReturnsFalse_WhenCardNotExists()
    {
        var cards = new List<Card> { new Card(10, 0), new Card(20, 1) };

        var result = CardCollectionHelper.HasCard(cards, 30);

        Assert.False(result);
    }
}
