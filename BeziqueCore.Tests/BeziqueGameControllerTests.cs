using BeziqueCore;

public class BeziqueGameControllerTests
{
    [Fact]
    public void Initialize_CreatesPlayersAndContext()
    {
        var controller = new BeziqueGameController();
        var config = new GameConfig { PlayerCount = 2, DeckCount = 4, TargetScore = 1500 };

        controller.Initialize(config);

        Assert.Equal(2, controller.Players.Length);
        Assert.NotNull(controller.Context);
        Assert.Equal(9, controller.Players[0].Hand.Count);
        Assert.Equal(9, controller.Players[1].Hand.Count);
    }

    [Fact]
    public void PlayCard_Phase1_ValidCard_RemovesFromHand()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, DeckCount = 4, TargetScore = 1500 });

        // Current turn player is 1 (after dealer), so get card from Player 1
        var card = controller.Players[1].Hand[0];
        int initialHandSize = controller.Players[1].Hand.Count;

        controller.PlayCard(card);

        Assert.Equal(initialHandSize - 1, controller.Players[1].Hand.Count);
        Assert.DoesNotContain(card, controller.Players[1].Hand);
    }

    [Fact]
    public void PlayCard_Phase1_InvalidCard_ReturnsFalse()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, DeckCount = 4, TargetScore = 1500 });

        // Use a card from deck index 99 which definitely won't be in any hand
        var card = new Card((byte)0, 99);
        bool result = controller.PlayCard(card);

        Assert.False(result);
    }

    [Fact]
    public void DeclareMeld_ValidMeld_AddsPoints()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, DeckCount = 4, TargetScore = 1500 });

        // Set current turn player to 0 so they can meld
        controller.Context.CurrentTurnPlayer = 0;

        // Set up player with empty hand to avoid other melds
        controller.Players[0].Hand.Clear();

        // Force trump to Diamonds and set trump card
        controller.Context.TrumpSuit = Suit.Diamonds;
        controller.Context.TrumpCard = new Card((byte)0, 99); // Not a Seven

        // Add King and Queen of Diamonds to player's hand
        // CardId 20 = King of Diamonds, CardId 16 = Queen of Diamonds
        var king = new Card((byte)20, 0);
        var queen = new Card((byte)16, 0);
        controller.Players[0].Hand.Add(king);
        controller.Players[0].Hand.Add(queen);

        int beforeScore = controller.Players[0].RoundScore;

        // Declare the meld - note: FindBestMeld will find Trump Marriage as the only/best meld
        controller.DeclareMeld(new[] { king, queen }, MeldType.TrumpMarriage);

        // Trump Marriage is 40 points
        Assert.Equal(beforeScore + 40, controller.Players[0].RoundScore);
    }

    [Fact]
    public void DeclareMeld_InvalidMeld_ReturnsFalse()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, DeckCount = 4, TargetScore = 1500 });

        // Try to meld with cards not in hand
        var king = new Card((byte)20, 99);
        var queen = new Card((byte)16, 99);
        bool result = controller.DeclareMeld(new[] { king, queen }, MeldType.TrumpMarriage);

        Assert.False(result);
    }

    [Fact]
    public void ResolveTrick_MovesCardsToWinner()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, DeckCount = 4, TargetScore = 1500 });

        controller.PlayCard(controller.Players[0].Hand[0]);
        controller.PlayCard(controller.Players[1].Hand[0]);

        int beforeWonPile = controller.Players[0].WonPile.Count + controller.Players[1].WonPile.Count;
        controller.ResolveTrick();

        Assert.True(controller.Players[0].WonPile.Count + controller.Players[1].WonPile.Count > beforeWonPile);
    }

    [Fact]
    public void StartNewTrick_ClearsPlayedCards()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, DeckCount = 4, TargetScore = 1500 });

        controller.PlayCard(controller.Players[0].Hand[0]);
        controller.PlayCard(controller.Players[1].Hand[0]);
        controller.ResolveTrick();

        controller.StartNewTrick();

        Assert.Empty(controller.PlayedCards);
    }

    [Fact]
    public void EndRound_AddsScoresToTotal()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, DeckCount = 4, TargetScore = 1500 });

        controller.Players[0].RoundScore = 100;
        controller.Players[1].RoundScore = 150;

        controller.EndRound();

        Assert.Equal(100, controller.Players[0].TotalScore);
        Assert.Equal(150, controller.Players[1].TotalScore);
    }

    [Fact]
    public void CheckWinner_PlayerAtTargetScore_ReturnsPlayerId()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, DeckCount = 4, TargetScore = 1500 });

        controller.Players[0].TotalScore = 1600;
        controller.Players[1].TotalScore = 1200;

        int winnerId = controller.CheckWinner();

        Assert.Equal(0, winnerId);
    }

    [Fact]
    public void CheckWinner_NoPlayerAtTargetScore_ReturnsNegativeOne()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, DeckCount = 4, TargetScore = 1500 });

        controller.Players[0].TotalScore = 1200;
        controller.Players[1].TotalScore = 1000;

        int winnerId = controller.CheckWinner();

        Assert.Equal(-1, winnerId);
    }

    [Fact]
    public void CanSwapTrumpSeven_PlayerHasTrumpSevenAndTrumpNotSeven_ReturnsTrue()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, DeckCount = 4, TargetScore = 1500 });

        // Set trump to Diamonds
        controller.Context.TrumpSuit = Suit.Diamonds;
        // Set trump card to something other than Seven (e.g., King of Diamonds = CardId 20)
        controller.Context.TrumpCard = new Card((byte)20, 0);

        // Clear hand and add Seven of Diamonds (CardId 0)
        controller.Players[0].Hand.Clear();
        controller.Players[0].Hand.Add(new Card((byte)0, 0));

        // Set current player to player 0
        controller.Context.CurrentTurnPlayer = 0;

        bool canSwap = controller.CanSwapTrumpSeven();

        Assert.True(canSwap);
    }

    [Fact]
    public void CanSwapTrumpSeven_PlayerDoesNotHaveTrumpSeven_ReturnsFalse()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, DeckCount = 4, TargetScore = 1500 });

        // Set trump to Diamonds
        controller.Context.TrumpSuit = Suit.Diamonds;
        controller.Context.TrumpCard = new Card((byte)20, 0);

        // Clear player's hand - no Seven of Diamonds
        controller.Players[0].Hand.Clear();
        controller.Context.CurrentTurnPlayer = 0;

        bool canSwap = controller.CanSwapTrumpSeven();

        Assert.False(canSwap);
    }

    [Fact]
    public void SwapTrumpSeven_ValidSwap_SwapsCardsAndAddsPoints()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, DeckCount = 4, TargetScore = 1500 });

        controller.Context.TrumpSuit = Suit.Diamonds;
        var originalTrumpCard = new Card((byte)20, 0); // King of Diamonds
        controller.Context.TrumpCard = originalTrumpCard;

        // Clear hand and add Seven of Diamonds
        controller.Players[0].Hand.Clear();
        var trumpSeven = new Card((byte)0, 0); // Seven of Diamonds
        controller.Players[0].Hand.Add(trumpSeven);
        controller.Context.CurrentTurnPlayer = 0;

        int beforeScore = controller.Players[0].RoundScore;
        bool swapped = controller.SwapTrumpSeven();

        Assert.True(swapped);
        Assert.Equal(beforeScore + 10, controller.Players[0].RoundScore);
        Assert.True(controller.Players[0].HasSwappedSeven);

        // Verify the swap happened - the original trump card should now be in player's hand
        bool hasOriginalTrump = false;
        foreach (var c in controller.Players[0].Hand)
        {
            if (c.CardId == originalTrumpCard.CardId && c.DeckIndex == originalTrumpCard.DeckIndex)
            {
                hasOriginalTrump = true;
                break;
            }
        }
        Assert.True(hasOriginalTrump);
    }
}
