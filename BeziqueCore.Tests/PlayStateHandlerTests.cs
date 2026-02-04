using BeziqueCore;

public class PlayStateHandlerTests
{
    [Fact]
    public void ValidateAndPlayCardPhase1_ValidCardInHand_ReturnsTrueAndAddsToPlayed()
    {
        var player = new Player(0) { Hand = new List<Card> { new Card((byte)10, 0) } };
        var playedCards = new List<Card>();

        bool result = PlayStateHandler.ValidateAndPlayCardPhase1(player, player.Hand[0], playedCards, 0);

        Assert.True(result);
        Assert.Single(playedCards);
        Assert.Empty(player.Hand);
    }

    [Fact]
    public void ValidateAndPlayCardPhase1_ValidCardInTableCards_ReturnsTrueAndAddsToPlayed()
    {
        var player = new Player(0)
        {
            Hand = new List<Card>(),
            TableCards = new List<Card> { new Card((byte)10, 0) }
        };
        var playedCards = new List<Card>();

        bool result = PlayStateHandler.ValidateAndPlayCardPhase1(player, player.TableCards[0], playedCards, 0);

        Assert.True(result);
        Assert.Single(playedCards);
        Assert.Empty(player.TableCards);
    }

    [Fact]
    public void ValidateAndPlayCardPhase1_CardNotInHandOrTableCards_ReturnsFalse()
    {
        var player = new Player(0) { Hand = new List<Card>() };
        var playedCards = new List<Card>();
        var card = new Card((byte)10, 0);

        bool result = PlayStateHandler.ValidateAndPlayCardPhase1(player, card, playedCards, 0);

        Assert.False(result);
        Assert.Empty(playedCards);
    }

    [Fact]
    public void ValidateAndPlayCardPhase1_PlayerIndexCorrect()
    {
        var player = new Player(1) { Hand = new List<Card> { new Card((byte)10, 0) } };
        var playedCards = new List<Card>();

        PlayStateHandler.ValidateAndPlayCardPhase1(player, player.Hand[0], playedCards, 1);

        Assert.Single(playedCards);
    }

    [Fact]
    public void ValidateAndPlayCardPhase1_MultiplePlays_AddsAllToPlayedCards()
    {
        var player = new Player(0)
        {
            Hand = new List<Card>
            {
                new Card((byte)10, 0),
                new Card((byte)11, 0)
            }
        };
        var playedCards = new List<Card>();

        PlayStateHandler.ValidateAndPlayCardPhase1(player, player.Hand[0], playedCards, 0);
        PlayStateHandler.ValidateAndPlayCardPhase1(player, player.Hand[0], playedCards, 0);

        Assert.Equal(2, playedCards.Count);
    }
}
