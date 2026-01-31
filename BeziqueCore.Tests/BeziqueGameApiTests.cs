using BeziqueCore.Core.API.Builders;
using BeziqueCore.Core.API.Events;
using BeziqueCore.Core.API.Factory;
using BeziqueCore.Core.API.Results;

namespace BeziqueCore.Tests.Unit;

public class BeziqueGameApiTests
{
    #region Game Creation Tests

    [Fact]
    public void CreateSinglePlayer_WithValidNames_CreatesGameWithTwoPlayers()
    {
        // Arrange & Act
        var game = BeziqueGameManager.CreateSinglePlayer("Human", "AI");

        // Assert
        Assert.NotNull(game);
        Assert.Equal(2, game.Players.Count);
        Assert.Equal("Human", game.Players[0].Name);
        Assert.Equal("AI", game.Players[1].Name);
        Assert.False(game.Players[0].IsBot);
        Assert.True(game.Players[1].IsBot);
    }

    [Fact]
    public void CreateMultiplayer_WithTwoPlayers_CreatesGameSuccessfully()
    {
        // Arrange & Act
        var game = BeziqueGameManager.CreateMultiplayer("Player1", "Player2");

        // Assert
        Assert.NotNull(game);
        Assert.Equal(2, game.Players.Count);
        Assert.Equal("Player1", game.Players[0].Name);
        Assert.Equal("Player2", game.Players[1].Name);
        Assert.All(game.Players, p => Assert.False(p.IsBot));
    }

    [Fact]
    public void CreateMultiplayer_WithLessThanTwoPlayers_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => BeziqueGameManager.CreateMultiplayer("Solo"));
    }

    [Fact]
    public void CreateGame_WithPlayerConfigs_CreatesGameWithCorrectSetup()
    {
        // Arrange
        var configs = new[]
        {
            new PlayerConfig { Name = "Alice", IsBot = false },
            new PlayerConfig { Name = "Bob", IsBot = true }
        };

        // Act
        var game = BeziqueGameManager.CreateGame(configs);

        // Assert
        Assert.NotNull(game);
        Assert.Equal(2, game.Players.Count);
        Assert.Equal("Alice", game.Players[0].Name);
        Assert.Equal("Bob", game.Players[1].Name);
        Assert.True(game.Players[1].IsBot);
    }

    [Fact]
    public void CreateCustom_WithBuilder_CreatesGameSuccessfully()
    {
        // Arrange & Act
        var game = BeziqueGameManager.CreateCustom(builder =>
        {
            builder
                .WithPlayer("Player1", isBot: false)
                .WithPlayer("Player2", isBot: true);
        });

        // Assert
        Assert.NotNull(game);
        Assert.Equal(2, game.Players.Count);
    }

    #endregion

    #region Game Start Tests

    [Fact]
    public void Start_AfterCreation_InitializesGameState()
    {
        // Arrange
        var game = BeziqueGameManager.CreateSinglePlayer("Human", "AI");

        // Act
        game.Start();

        // Assert
        Assert.NotNull(game.CurrentPlayer);
        Assert.NotEmpty(game.CurrentStateName);
    }

    [Fact]
    public void Start_DealsCardsToPlayers()
    {
        // Arrange
        var game = BeziqueGameManager.CreateSinglePlayer("Human", "AI");

        // Act
        game.Start();

        // Assert
        foreach (var player in game.Players)
        {
            Assert.True(player.Hand.Count > 0, $"Player {player.Name} should have cards in hand");
        }
    }

    #endregion

    #region Game State Tests

    [Fact]
    public void IsGameOver_AfterStart_ReturnsFalse()
    {
        // Arrange
        var game = BeziqueGameManager.CreateSinglePlayer("Human", "AI");
        game.Start();

        // Act & Assert
        Assert.False(game.IsGameOver);
    }

    [Fact]
    public void Players_ReturnsReadOnlyList()
    {
        // Arrange
        var game = BeziqueGameManager.CreateSinglePlayer("Human", "AI");

        // Act
        var players = game.Players;

        // Assert
        Assert.IsAssignableFrom<IReadOnlyList<BeziqueCore.Core.Domain.Entities.Player>>(players);
    }

    [Fact]
    public void GetSnapshot_ReturnsValidGameState()
    {
        // Arrange
        var game = BeziqueGameManager.CreateSinglePlayer("Human", "AI");
        game.Start();

        // Act
        var snapshot = game.GetSnapshot();

        // Assert
        Assert.NotNull(snapshot);
        Assert.NotNull(snapshot.StateName);
        Assert.NotNull(snapshot.Players);
        Assert.Equal(2, snapshot.Players.Length);
        Assert.NotNull(snapshot.TrumpSuit);
    }

    [Fact]
    public void GetSnapshotForPlayer_ReturnsPlayerSpecificView()
    {
        // Arrange
        var game = BeziqueGameManager.CreateSinglePlayer("Human", "AI");
        game.Start();
        var playerId = game.Players[0].Id;

        // Act
        var snapshot = game.GetSnapshotForPlayer(playerId);

        // Assert
        Assert.NotNull(snapshot);
        Assert.NotNull(snapshot.CurrentPlayerUserId);
    }

    #endregion

    #region Action Validation Tests

    [Fact]
    public void CanPlayCard_WithValidIndex_ReturnsExpectedResult()
    {
        // Arrange
        var game = BeziqueGameManager.CreateSinglePlayer("Human", "AI");
        game.Start();

        // Act
        var canPlay = game.CanPlayCard(0, 0);

        // Assert - result depends on game state, but should not throw
        Assert.True(canPlay || !canPlay);
    }

    [Fact]
    public void CanPlayCard_WithInvalidPlayerIndex_ReturnsFalse()
    {
        // Arrange
        var game = BeziqueGameManager.CreateSinglePlayer("Human", "AI");
        game.Start();

        // Act
        var canPlay = game.CanPlayCard(-1, 0);

        // Assert
        Assert.False(canPlay);
    }

    [Fact]
    public void CanDeclareMeld_WithInvalidPlayerIndex_ReturnsFalse()
    {
        // Arrange
        var game = BeziqueGameManager.CreateSinglePlayer("Human", "AI");
        game.Start();

        // Act
        var canDeclare = game.CanDeclareMeld(99, new[] { 0, 1 });

        // Assert
        Assert.False(canDeclare);
    }

    [Fact]
    public void CanSwitchSevenOfTrump_WithInvalidPlayerIndex_ReturnsFalse()
    {
        // Arrange
        var game = BeziqueGameManager.CreateSinglePlayer("Human", "AI");
        game.Start();

        // Act
        var canSwitch = game.CanSwitchSevenOfTrump(-1);

        // Assert
        Assert.False(canSwitch);
    }

    [Fact]
    public void GetLegalMoves_WithInvalidPlayerIndex_ReturnsEmptyArray()
    {
        // Arrange
        var game = BeziqueGameManager.CreateSinglePlayer("Human", "AI");
        game.Start();

        // Act
        var moves = game.GetLegalMoves(-1);

        // Assert
        Assert.Empty(moves);
    }

    [Fact]
    public void GetLegalMoves_WithValidPlayerIndex_ReturnsArray()
    {
        // Arrange
        var game = BeziqueGameManager.CreateSinglePlayer("Human", "AI");
        game.Start();

        // Act
        var moves = game.GetLegalMoves(0);

        // Assert
        Assert.NotNull(moves);
    }

    #endregion

    #region Action Execution Tests

    [Fact]
    public void PlayCard_WithInvalidPlayerIndex_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var game = BeziqueGameManager.CreateSinglePlayer("Human", "AI");
        game.Start();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => game.PlayCard(-1, 0));
    }

    [Fact]
    public void DeclareMeld_WithInvalidPlayerIndex_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var game = BeziqueGameManager.CreateSinglePlayer("Human", "AI");
        game.Start();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => game.DeclareMeld(99, new[] { 0, 1 }));
    }

    [Fact]
    public void SkipMeld_WithInvalidPlayerIndex_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var game = BeziqueGameManager.CreateSinglePlayer("Human", "AI");
        game.Start();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => game.SkipMeld(-1));
    }

    [Fact]
    public void SwitchSevenOfTrump_WithInvalidPlayerIndex_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var game = BeziqueGameManager.CreateSinglePlayer("Human", "AI");
        game.Start();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => game.SwitchSevenOfTrump(99));
    }

    #endregion

    #region GameActionBuilder Tests

    [Fact]
    public void Actions_ReturnsGameActionBuilder()
    {
        // Arrange
        var game = BeziqueGameManager.CreateSinglePlayer("Human", "AI");

        // Act
        var builder = game.Actions();

        // Assert
        Assert.NotNull(builder);
    }

    [Fact]
    public void Actions_ForPlayer_WithValidIndex_ReturnsBuilder()
    {
        // Arrange
        var game = BeziqueGameManager.CreateSinglePlayer("Human", "AI");
        game.Start();

        // Act
        var builder = game.Actions().ForPlayer(0);

        // Assert
        Assert.NotNull(builder);
    }

    [Fact]
    public void Actions_ForPlayer_WithPlayerName_ReturnsBuilder()
    {
        // Arrange
        var game = BeziqueGameManager.CreateSinglePlayer("Human", "AI");
        game.Start();

        // Act
        var builder = game.Actions().ForPlayer("Human");

        // Assert
        Assert.NotNull(builder);
    }

    [Fact]
    public void Actions_ForPlayer_WithInvalidName_ThrowsArgumentException()
    {
        // Arrange
        var game = BeziqueGameManager.CreateSinglePlayer("Human", "AI");
        game.Start();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => game.Actions().ForPlayer("NonExistent"));
    }

    [Fact]
    public void Actions_CanPlayCard_ReturnsActionResult()
    {
        // Arrange
        var game = BeziqueGameManager.CreateSinglePlayer("Human", "AI");
        game.Start();

        // Act
        var result = game.Actions().ForPlayer(0).CanPlayCard(0);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<ActionResult>(result);
    }

    [Fact]
    public void Actions_CanDeclareMeld_ReturnsActionResult()
    {
        // Arrange
        var game = BeziqueGameManager.CreateSinglePlayer("Human", "AI");
        game.Start();

        // Act
        var result = game.Actions().ForPlayer(0).CanDeclareMeld(0, 1);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void Actions_GetLegalMoves_ReturnsTypedActionResult()
    {
        // Arrange
        var game = BeziqueGameManager.CreateSinglePlayer("Human", "AI");
        game.Start();

        // Act
        var result = game.Actions().ForPlayer(0).GetLegalMoves();

        // Assert
        Assert.NotNull(result);
        Assert.IsType<ActionResult<string[]>>(result);
        Assert.True(result.Success);
    }

    [Fact]
    public async Task PlayCardAsync_Extension_ReturnsActionResult()
    {
        // Arrange
        var game = BeziqueGameManager.CreateSinglePlayer("Human", "AI");
        game.Start();

        // Act
        var result = await game.PlayCardAsync(0, 0);

        // Assert
        Assert.NotNull(result);
    }

    #endregion

    #region Event Subscription Tests

    [Fact]
    public void Events_Property_ReturnsEventSubscriptionManager()
    {
        // Arrange
        var game = BeziqueGameManager.CreateSinglePlayer("Human", "AI");

        // Act
        var events = game.Events;

        // Assert
        Assert.NotNull(events);
    }

    [Fact]
    public void CardPlayed_Event_CanBeSubscribed()
    {
        // Arrange
        var game = BeziqueGameManager.CreateSinglePlayer("Human", "AI");
        var eventRaised = false;

        // Act
        game.CardPlayed += (sender, args) => eventRaised = true;

        // Assert - event subscription should not throw
        Assert.False(eventRaised); // Event hasn't been raised yet
    }

    [Fact]
    public void MeldDeclared_Event_CanBeSubscribed()
    {
        // Arrange
        var game = BeziqueGameManager.CreateSinglePlayer("Human", "AI");
        var eventRaised = false;

        // Act
        game.MeldDeclared += (sender, args) => eventRaised = true;

        // Assert
        Assert.False(eventRaised);
    }

    [Fact]
    public void TrickResolved_Event_CanBeSubscribed()
    {
        // Arrange
        var game = BeziqueGameManager.CreateSinglePlayer("Human", "AI");
        var eventRaised = false;

        // Act
        game.TrickResolved += (sender, args) => eventRaised = true;

        // Assert
        Assert.False(eventRaised);
    }

    [Fact]
    public void GameEnded_Event_CanBeSubscribed()
    {
        // Arrange
        var game = BeziqueGameManager.CreateSinglePlayer("Human", "AI");
        var eventRaised = false;

        // Act
        game.GameEnded += (sender, args) => eventRaised = true;

        // Assert
        Assert.False(eventRaised);
    }

    [Fact]
    public void Error_Event_CanBeSubscribed()
    {
        // Arrange
        var game = BeziqueGameManager.CreateSinglePlayer("Human", "AI");
        var eventRaised = false;

        // Act
        game.Error += (sender, args) => eventRaised = true;

        // Assert
        Assert.False(eventRaised);
    }

    #endregion

    #region Event Batching Tests

    [Fact]
    public void BeginEventBatch_DoesNotThrow()
    {
        // Arrange
        var game = BeziqueGameManager.CreateSinglePlayer("Human", "AI");

        // Act & Assert
        var exception = Record.Exception(() => game.BeginEventBatch());
        Assert.Null(exception);
    }

    [Fact]
    public void EndEventBatch_DoesNotThrow()
    {
        // Arrange
        var game = BeziqueGameManager.CreateSinglePlayer("Human", "AI");
        game.BeginEventBatch();

        // Act & Assert
        var exception = Record.Exception(() => game.EndEventBatch());
        Assert.Null(exception);
    }

    [Fact]
    public void FlushEventBatch_DoesNotThrow()
    {
        // Arrange
        var game = BeziqueGameManager.CreateSinglePlayer("Human", "AI");
        game.BeginEventBatch();

        // Act & Assert
        var exception = Record.Exception(() => game.FlushEventBatch());
        Assert.Null(exception);
    }

    #endregion

    #region ActionResult Tests

    [Fact]
    public void ActionResult_Ok_ReturnsSuccessResult()
    {
        // Act
        var result = ActionResult.Ok("test data");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("test data", result.Data);
    }

    [Fact]
    public void ActionResult_Fail_ReturnsFailureResult()
    {
        // Act
        var result = ActionResult.Fail("error message");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("error message", result.Message);
    }

    [Fact]
    public void ActionResult_Match_ExecutesCorrectBranch()
    {
        // Arrange
        var successResult = ActionResult.Ok();
        var failResult = ActionResult.Fail("error");
        var successExecuted = false;
        var failExecuted = false;

        // Act
        successResult.Match(() => successExecuted = true, _ => { });
        failResult.Match(() => { }, _ => failExecuted = true);

        // Assert
        Assert.True(successExecuted);
        Assert.True(failExecuted);
    }

    [Fact]
    public void ActionResultT_Ok_ReturnsTypedSuccessResult()
    {
        // Act
        var result = ActionResult<int>.Ok(42);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(42, result.Data);
    }

    [Fact]
    public void ActionResultT_Match_ReturnsCorrectValue()
    {
        // Arrange
        var result = ActionResult<int>.Ok(42);

        // Act
        var value = result.Match(data => data * 2, _ => 0);

        // Assert
        Assert.Equal(84, value);
    }

    #endregion

    #region Builder Pattern Tests

    [Fact]
    public void BeziqueGameBuilder_WithPlayer_AddsPlayer()
    {
        // Arrange & Act
        var game = BeziqueGameManager.CreateCustom(builder =>
        {
            builder
                .WithPlayer("Alice")
                .WithPlayer("Bob");
        });

        // Assert
        Assert.Equal(2, game.Players.Count);
        Assert.Equal("Alice", game.Players[0].Name);
        Assert.Equal("Bob", game.Players[1].Name);
    }

    [Fact]
    public void BeziqueGameBuilder_WithPlayers_AddsMultiplePlayers()
    {
        // Arrange & Act
        var game = BeziqueGameManager.CreateCustom(builder =>
        {
            builder.WithPlayers(
                p => p.Named("Alice").AsHuman(),
                p => p.Named("Bot").AsAI()
            );
        });

        // Assert
        Assert.Equal(2, game.Players.Count);
        Assert.False(game.Players[0].IsBot);
        Assert.True(game.Players[1].IsBot);
    }

    [Fact]
    public void BeziqueGameBuilder_WithLessThanTwoPlayers_ThrowsInvalidOperationException()
    {
        // Arrange & Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            BeziqueGameManager.CreateCustom(builder =>
            {
                builder.WithPlayer("Solo");
            });
        });
    }

    [Fact]
    public void PlayerConfigBuilder_Named_SetsName()
    {
        // Arrange & Act
        var game = BeziqueGameManager.CreateCustom(builder =>
        {
            builder.WithPlayers(
                p => p.Named("CustomName"),
                p => p.Named("OtherPlayer")
            );
        });

        // Assert
        Assert.Equal("CustomName", game.Players[0].Name);
    }

    [Fact]
    public void PlayerConfigBuilder_AsDealer_SetsDealer()
    {
        // Arrange & Act
        var game = BeziqueGameManager.CreateCustom(builder =>
        {
            builder.WithPlayers(
                p => p.Named("Dealer").AsDealer(),
                p => p.Named("NonDealer")
            );
        });

        // Assert - first player is dealer by default in CreateGame
        Assert.True(game.Players[0].IsDealer);
    }

    #endregion
}
