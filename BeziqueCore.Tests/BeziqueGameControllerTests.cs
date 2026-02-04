using BeziqueCore;

public class BeziqueGameControllerTests
{
    [Fact]
    public void Initialize_CreatesPlayersAndContext()
    {
        var controller = new BeziqueGameController();
        var config = new GameConfig { PlayerCount = 2, DeckCount = 2, TargetScore = 1500 };

        controller.Initialize(config);

        Assert.Equal(2, controller.Players.Length);
        Assert.NotNull(controller.Context);
        Assert.Equal(9, controller.Players[0].Hand.Count);
        Assert.Equal(9, controller.Players[1].Hand.Count);
        Assert.Equal(GameState.Play, controller.CurrentState);
    }

    [Fact]
    public void PlayCard_Phase1_ValidCard_ReturnsTrue()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, DeckCount = 2, TargetScore = 1500 });

        var card = controller.Players[0].Hand[0];
        bool result = controller.PlayCard(card);

        Assert.True(result);
        Assert.Single(controller.PlayedCards);
        Assert.Equal(8, controller.Players[0].Hand.Count);
    }

    [Fact]
    public void PlayCard_Phase1_AllPlayersPlayed_TransitionsToMeld()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, DeckCount = 2, TargetScore = 1500 });

        controller.PlayCard(controller.Players[0].Hand[0]);
        controller.PlayCard(controller.Players[1].Hand[0]);

        Assert.Equal(GameState.Meld, controller.CurrentState);
    }

    [Fact]
    public void PlayCard_InvalidState_ReturnsFalse()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, DeckCount = 2, TargetScore = 1500 });
        controller.CurrentState = GameState.RoundEnd;

        bool result = controller.PlayCard(controller.Players[0].Hand[0]);

        Assert.False(result);
    }

    [Fact]
    public void PlayCard_Phase1_InvalidCard_ReturnsFalse()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, DeckCount = 2, TargetScore = 1500 });

        // Use a card from deck index 99 which definitely won't be in any hand
        var card = new Card((byte)0, 99);
        bool result = controller.PlayCard(card);

        Assert.False(result);
    }

    [Fact]
    public void DeclareMeld_ValidMeld_ReturnsTrue()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, DeckCount = 2, TargetScore = 1500 });

        // Force trump to Diamonds
        controller.Context.TrumpSuit = Suit.Diamonds;

        controller.PlayCard(controller.Players[0].Hand[0]);
        controller.PlayCard(controller.Players[1].Hand[0]);

        // It's Player 1's turn now
        var king = new Card((byte)20, 0);
        var queen = new Card((byte)16, 0);
        controller.Players[1].Hand.Add(king);
        controller.Players[1].Hand.Add(queen);

        bool result = controller.DeclareMeld(new[] { king, queen }, MeldType.TrumpMarriage);

        Assert.True(result);
        Assert.Equal(40, controller.Players[1].RoundScore);
    }

    [Fact]
    public void DeclareMeld_WrongState_ReturnsFalse()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, DeckCount = 2, TargetScore = 1500 });

        var king = new Card((byte)20, 0);
        var queen = new Card((byte)16, 0);
        bool result = controller.DeclareMeld(new[] { king, queen }, MeldType.TrumpMarriage);

        Assert.False(result);
    }

    [Fact]
    public void ResolveTrick_MovesCardsToWinner()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, DeckCount = 2, TargetScore = 1500 });

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
        controller.Initialize(new GameConfig { PlayerCount = 2, DeckCount = 2, TargetScore = 1500 });

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
        controller.Initialize(new GameConfig { PlayerCount = 2, DeckCount = 2, TargetScore = 1500 });

        controller.Players[0].RoundScore = 100;
        controller.Players[1].RoundScore = 150;
        controller.CurrentState = GameState.RoundEnd;

        controller.EndRound();

        Assert.Equal(100, controller.Players[0].TotalScore);
        Assert.Equal(150, controller.Players[1].TotalScore);
    }

    [Fact]
    public void EndRound_GameOver_ReturnsWinnerId()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, DeckCount = 2, TargetScore = 1500 });

        controller.Players[0].RoundScore = 100;
        controller.Players[1].RoundScore = 150;
        controller.Players[0].TotalScore = 1400;
        controller.Players[1].TotalScore = 1350;
        controller.CurrentState = GameState.RoundEnd;

        int winnerId = controller.EndRound();

        Assert.Equal(0, winnerId);
        Assert.Equal(GameState.GameOver, controller.CurrentState);
    }

    [Fact]
    public void EndRound_NotGameOver_StartsNewRound()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, DeckCount = 2, TargetScore = 1500 });

        controller.Players[0].RoundScore = 100;
        controller.Players[1].RoundScore = 150;
        controller.CurrentState = GameState.RoundEnd;

        controller.EndRound();

        Assert.Equal(GameState.Play, controller.CurrentState);
        Assert.Equal(9, controller.Players[0].Hand.Count);
        Assert.Equal(9, controller.Players[1].Hand.Count);
    }

    [Fact]
    public void PlayCard_AdvancesTurn()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 2, DeckCount = 2, TargetScore = 1500 });

        int initialTurn = controller.Context.CurrentTurnPlayer;
        controller.PlayCard(controller.Players[0].Hand[0]);

        Assert.NotEqual(initialTurn, controller.Context.CurrentTurnPlayer);
    }

    [Fact]
    public void PlayCard_FourPlayers_AdvancesTurnCorrectly()
    {
        var controller = new BeziqueGameController();
        controller.Initialize(new GameConfig { PlayerCount = 4, DeckCount = 2, TargetScore = 1500 });

        Assert.Equal(0, controller.Context.CurrentTurnPlayer);
        controller.PlayCard(controller.Players[0].Hand[0]);
        Assert.Equal(1, controller.Context.CurrentTurnPlayer);
        controller.PlayCard(controller.Players[1].Hand[0]);
        Assert.Equal(2, controller.Context.CurrentTurnPlayer);
        controller.PlayCard(controller.Players[2].Hand[0]);
        Assert.Equal(3, controller.Context.CurrentTurnPlayer);
        controller.PlayCard(controller.Players[3].Hand[0]);
        // After all 4 players played, state transitions to Meld, turn stays at 3
        Assert.Equal(3, controller.Context.CurrentTurnPlayer);
        Assert.Equal(GameState.Meld, controller.CurrentState);
    }
}
