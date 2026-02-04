using BeziqueCore;

public class GameInitializerTests
{
    [Fact]
    public void InitializeGame_TwoPlayers_CreatesCorrectSetup()
    {
        var config = new GameConfig { PlayerCount = 2 };
        GameInitializer.InitializeGame(config, out var players, out var context);

        Assert.Equal(2, players.Length);
        Assert.Equal(9, players[0].Hand.Count);
        Assert.Equal(9, players[1].Hand.Count);
        Assert.Equal(0, players[0].PlayerID);
        Assert.Equal(1, players[1].PlayerID);
        Assert.False(context.TrumpCard.IsJoker);
        Assert.Equal(0, context.CurrentTurnPlayer);
        Assert.Equal(GamePhase.Phase1_Normal, context.CurrentPhase);
    }

    [Fact]
    public void InitializeGame_FourPlayers_CreatesCorrectSetup()
    {
        var config = new GameConfig { PlayerCount = 4 };
        GameInitializer.InitializeGame(config, out var players, out var context);

        Assert.Equal(4, players.Length);
        Assert.All(players, p => Assert.Equal(9, p.Hand.Count));
        int expectedDeckCount = 132 - 36 - 1;
        Assert.Equal(expectedDeckCount, context.DrawDeck.Count);
    }

    [Fact]
    public void InitializeGame_TrumpIsSeven_AwardsDealerBonus()
    {
        var config = new GameConfig { PlayerCount = 2 };
        GameInitializer.InitializeGame(config, out var players, out var context);

        if (context.TrumpCard.Rank == Rank.Seven)
        {
            Assert.Equal(10, players[1].RoundScore);
        }
    }

    [Fact]
    public void InitializeGame_TrumpNotSeven_NoDealerBonus()
    {
        var config = new GameConfig { PlayerCount = 2 };
        GameInitializer.InitializeGame(config, out var players, out var context);

        if (context.TrumpCard.Rank != Rank.Seven)
        {
            Assert.Equal(0, players[1].RoundScore);
        }
    }

    [Fact]
    public void InitializeGame_SetsTrumpSuitFromTrumpCard()
    {
        var config = new GameConfig { PlayerCount = 2 };
        GameInitializer.InitializeGame(config, out var players, out var context);

        if (!context.TrumpCard.IsJoker)
        {
            Assert.Equal(context.TrumpCard.Suit, context.TrumpSuit);
        }
    }

    [Fact]
    public void InitializeGame_AllPlayersHaveEmptyState_ExceptHand()
    {
        var config = new GameConfig { PlayerCount = 2 };
        GameInitializer.InitializeGame(config, out var players, out var context);

        Assert.All(players, p =>
        {
            Assert.Empty(p.TableCards);
            Assert.Empty(p.WonPile);
            Assert.Equal(0, p.TotalScore);
            Assert.False(p.HasSwappedSeven);
            Assert.Empty(p.MeldHistory);
        });
    }

    [Fact]
    public void InitializeGame_DrawDeckHasCorrectCardCount()
    {
        var config = new GameConfig { PlayerCount = 2, DeckCount = 4 };
        GameInitializer.InitializeGame(config, out var players, out var context);

        int expectedDeckCount = 132 - 18 - 1;
        Assert.Equal(expectedDeckCount, context.DrawDeck.Count);
    }

    [Fact]
    public void InitializeGame_FourPlayers_DrawDeckHasCorrectCardCount()
    {
        var config = new GameConfig { PlayerCount = 4, DeckCount = 4 };
        GameInitializer.InitializeGame(config, out var players, out var context);

        int expectedDeckCount = 132 - 36 - 1;
        Assert.Equal(expectedDeckCount, context.DrawDeck.Count);
    }
}
