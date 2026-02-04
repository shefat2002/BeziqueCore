using BeziqueCore;
using BeziqueCore.Interfaces;

public class DeckTests
{
    [Fact]
    public void DeckFactory_Create_ReturnsCorrectCardCount()
    {
        var deck = DeckFactory.CreateDeck(4);
        Assert.Equal(132, deck.Length);
    }

    [Fact]
    public void DeckFactory_Create_ContainsUniqueAcesOfSpades()
    {
        var deck = DeckFactory.CreateDeck(4);
        var acesOfSpades = deck.Where(c => c.CardId == 31 && c.Suit == Suit.Spades && c.Rank == Rank.Ace).ToArray();

        Assert.Equal(4, acesOfSpades.Length);
        Assert.Equal(4, acesOfSpades.Select(c => c.DeckIndex).Distinct().Count());
    }

    [Fact]
    public void DeckFactory_Create_ContainsUniqueJokers()
    {
        var deck = DeckFactory.CreateDeck(4);
        var jokers = deck.Where(c => c.IsJoker).ToArray();

        Assert.Equal(4, jokers.Length);
        Assert.Equal(4, jokers.Select(c => c.DeckIndex).Distinct().Count());
    }

    [Fact]
    public void Card_ComputedProperties_ReturnCorrectValues()
    {
        var sevenDiamonds = new Card(0, 0);
        Assert.Equal(Suit.Diamonds, sevenDiamonds.Suit);
        Assert.Equal(Rank.Seven, sevenDiamonds.Rank);
        Assert.False(sevenDiamonds.IsJoker);

        var aceSpades = new Card(31, 0);
        Assert.Equal(Suit.Spades, aceSpades.Suit);
        Assert.Equal(Rank.Ace, aceSpades.Rank);
        Assert.False(aceSpades.IsJoker);

        var joker = new Card(32, 0);
        Assert.True(joker.IsJoker);
        Assert.Equal((Rank)15, joker.Rank);
    }

    [Fact]
    public void DeckFactory_Shuffle_PreservesAllCards()
    {
        var random = new SystemRandom();
        var deck = DeckFactory.Shuffled(random, 4);

        Assert.Equal(132, deck.Length);
        Assert.Equal(4, deck.Count(c => c.CardId == 31 && c.Suit == Suit.Spades && c.Rank == Rank.Ace));
    }

    [Fact]
    public void Player_Constructor_InitializesWithDefaultValues()
    {
        var player = new Player(0);

        Assert.Equal(0, player.PlayerID);
        Assert.Empty(player.Hand);
        Assert.Empty(player.TableCards);
        Assert.Empty(player.WonPile);
        Assert.Equal(0, player.RoundScore);
        Assert.Equal(0, player.TotalScore);
        Assert.False(player.HasSwappedSeven);
        Assert.Empty(player.MeldHistory);
    }

    [Fact]
    public void GameContext_Constructor_InitializesWithDefaultValues()
    {
        var context = new GameContext();

        Assert.NotNull(context.DrawDeck);
        Assert.Equal(GamePhase.Phase1_Normal, context.CurrentPhase);
        Assert.Equal(0, context.CurrentTurnPlayer);
        Assert.Equal(0, context.LastTrickWinner);
    }

    [Fact]
    public void GameConfig_Standard_ReturnsDefaultConfiguration()
    {
        var config = GameConfig.Standard;

        Assert.Equal(2, config.PlayerCount);
        Assert.Equal(GameMode.Standard, config.Mode);
        Assert.Equal((ushort)1500, config.TargetScore);
        Assert.Equal((byte)4, config.DeckCount);
    }

    [Fact]
    public void Card_Equality_ComparisonWorksCorrectly()
    {
        var card1 = new Card(10, 0);
        var card2 = new Card(10, 0);
        var card3 = new Card(10, 1);
        var card4 = new Card(11, 0);

        Assert.Equal(card1, card2);
        Assert.NotEqual(card1, card3);
        Assert.NotEqual(card1, card4);
    }
}
