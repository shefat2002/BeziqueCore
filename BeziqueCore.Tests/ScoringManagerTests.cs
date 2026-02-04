using BeziqueCore;

public class ScoringManagerTests
{
    #region CountAcesAndTens Tests

    [Fact]
    public void CountAcesAndTens_EmptyPile_ReturnsZero()
    {
        var wonPile = new List<Card>();
        Assert.Equal(0, ScoringManager.CountAcesAndTens(wonPile));
    }

    [Fact]
    public void CountAcesAndTens_NoAcesOrTens_ReturnsZero()
    {
        var wonPile = new List<Card>
        {
            new Card((byte)0, 0),
            new Card((byte)4, 0),
            new Card((byte)8, 0)
        };
        Assert.Equal(0, ScoringManager.CountAcesAndTens(wonPile));
    }

    [Fact]
    public void CountAcesAndTens_OnlyAces_ReturnsAceCount()
    {
        var wonPile = new List<Card>
        {
            new Card((byte)28, 0),
            new Card((byte)29, 0),
            new Card((byte)30, 0)
        };
        Assert.Equal(3, ScoringManager.CountAcesAndTens(wonPile));
    }

    [Fact]
    public void CountAcesAndTens_OnlyTens_ReturnsTenCount()
    {
        var wonPile = new List<Card>
        {
            new Card((byte)24, 0),
            new Card((byte)25, 0),
            new Card((byte)26, 0),
            new Card((byte)27, 0)
        };
        Assert.Equal(4, ScoringManager.CountAcesAndTens(wonPile));
    }

    [Fact]
    public void CountAcesAndTens_MixedCards_ReturnsCorrectCount()
    {
        var wonPile = new List<Card>
        {
            new Card((byte)0, 0),
            new Card((byte)24, 0),
            new Card((byte)4, 0),
            new Card((byte)28, 0),
            new Card((byte)8, 0),
            new Card((byte)25, 0)
        };
        Assert.Equal(3, ScoringManager.CountAcesAndTens(wonPile));
    }

    [Fact]
    public void CountAcesAndTens_AllAcesAndTens_ReturnsTotalCount()
    {
        var wonPile = new List<Card>();
        for (sbyte i = 0; i < 4; i++)
        {
            wonPile.Add(new Card((byte)28, i));
            wonPile.Add(new Card((byte)24, i));
        }
        Assert.Equal(8, ScoringManager.CountAcesAndTens(wonPile));
    }

    [Fact]
    public void CountAcesAndTens_IgnoresJokers()
    {
        var wonPile = new List<Card>
        {
            new Card((byte)28, 0),
            new Card((byte)32, 0),
            new Card((byte)24, 0)
        };
        Assert.Equal(2, ScoringManager.CountAcesAndTens(wonPile));
    }

    #endregion

    #region CalculateAdvancedBonus Tests

    [Fact]
    public void CalculateAdvancedBonus_TwoPlayers_BelowThreshold_ReturnsZero()
    {
        Assert.Equal(0, ScoringManager.CalculateAdvancedBonus(aceTenCount: 13, playerCount: 2));
    }

    [Fact]
    public void CalculateAdvancedBonus_TwoPlayers_AtThreshold_ReturnsBonus()
    {
        Assert.Equal(140, ScoringManager.CalculateAdvancedBonus(aceTenCount: 14, playerCount: 2));
    }

    [Fact]
    public void CalculateAdvancedBonus_TwoPlayers_AboveThreshold_ReturnsBonus()
    {
        Assert.Equal(150, ScoringManager.CalculateAdvancedBonus(aceTenCount: 15, playerCount: 2));
    }

    [Fact]
    public void CalculateAdvancedBonus_TwoPlayers_MaxBonus_ReturnsCorrectAmount()
    {
        Assert.Equal(250, ScoringManager.CalculateAdvancedBonus(aceTenCount: 25, playerCount: 2));
    }

    [Fact]
    public void CalculateAdvancedBonus_FourPlayers_BelowThreshold_ReturnsZero()
    {
        Assert.Equal(0, ScoringManager.CalculateAdvancedBonus(aceTenCount: 7, playerCount: 4));
    }

    [Fact]
    public void CalculateAdvancedBonus_FourPlayers_AtThreshold_ReturnsBonus()
    {
        Assert.Equal(80, ScoringManager.CalculateAdvancedBonus(aceTenCount: 8, playerCount: 4));
    }

    [Fact]
    public void CalculateAdvancedBonus_FourPlayers_AboveThreshold_ReturnsBonus()
    {
        Assert.Equal(90, ScoringManager.CalculateAdvancedBonus(aceTenCount: 9, playerCount: 4));
    }

    [Fact]
    public void CalculateAdvancedBonus_FourPlayers_MaxBonus_ReturnsCorrectAmount()
    {
        Assert.Equal(160, ScoringManager.CalculateAdvancedBonus(aceTenCount: 16, playerCount: 4));
    }

    [Fact]
    public void CalculateAdvancedBonus_ZeroCount_ReturnsZero()
    {
        Assert.Equal(0, ScoringManager.CalculateAdvancedBonus(aceTenCount: 0, playerCount: 2));
        Assert.Equal(0, ScoringManager.CalculateAdvancedBonus(aceTenCount: 0, playerCount: 4));
    }

    #endregion

    #region CalculateRoundScores Tests

    [Fact]
    public void CalculateRoundScores_StandardMode_AddsOnlyRoundScore()
    {
        var players = new[]
        {
            new Player(0) { RoundScore = 100, TotalScore = 500, WonPile = new List<Card>() },
            new Player(1) { RoundScore = 150, TotalScore = 600, WonPile = new List<Card>() }
        };

        ScoringManager.CalculateRoundScores(players, GameMode.Standard, playerCount: 2);

        Assert.Equal(600, players[0].TotalScore);
        Assert.Equal(750, players[1].TotalScore);
    }

    [Fact]
    public void CalculateRoundScores_AdvancedMode_BelowThreshold_AddsOnlyRoundScore()
    {
        var players = new[]
        {
            new Player(0)
            {
                RoundScore = 100,
                TotalScore = 500,
                WonPile = new List<Card> { new Card((byte)28, 0) }
            }
        };

        ScoringManager.CalculateRoundScores(players, GameMode.Advanced, playerCount: 2);

        Assert.Equal(600, players[0].TotalScore);
    }

    [Fact]
    public void CalculateRoundScores_AdvancedMode_AtThreshold_AddsRoundScorePlusBonus()
    {
        var wonPile = new List<Card>();
        for (int i = 0; i < 14; i++)
        {
            wonPile.Add(new Card((byte)28, (sbyte)(i % 4)));
        }

        var players = new[]
        {
            new Player(0)
            {
                RoundScore = 100,
                TotalScore = 500,
                WonPile = wonPile
            }
        };

        ScoringManager.CalculateRoundScores(players, GameMode.Advanced, playerCount: 2);

        Assert.Equal(740, players[0].TotalScore);
    }

    [Fact]
    public void CalculateRoundScores_AdvancedMode_FourPlayersAtThreshold_AddsRoundScorePlusBonus()
    {
        var wonPile = new List<Card>();
        for (int i = 0; i < 8; i++)
        {
            wonPile.Add(new Card((byte)28, (sbyte)(i % 4)));
        }

        var players = new[]
        {
            new Player(0)
            {
                RoundScore = 100,
                TotalScore = 500,
                WonPile = wonPile
            }
        };

        ScoringManager.CalculateRoundScores(players, GameMode.Advanced, playerCount: 4);

        Assert.Equal(680, players[0].TotalScore);
    }

    [Fact]
    public void CalculateRoundScores_AdvancedMode_MultiplePlayers_CalculatesEachIndependently()
    {
        var wonPile1 = new List<Card>();
        for (int i = 0; i < 14; i++) wonPile1.Add(new Card((byte)28, (sbyte)(i % 4)));

        var wonPile2 = new List<Card>();
        for (int i = 0; i < 5; i++) wonPile2.Add(new Card((byte)28, (sbyte)(i % 4)));

        var players = new[]
        {
            new Player(0)
            {
                RoundScore = 100,
                TotalScore = 500,
                WonPile = wonPile1
            },
            new Player(1)
            {
                RoundScore = 150,
                TotalScore = 600,
                WonPile = wonPile2
            }
        };

        ScoringManager.CalculateRoundScores(players, GameMode.Advanced, playerCount: 2);

        Assert.Equal(740, players[0].TotalScore);
        Assert.Equal(750, players[1].TotalScore);
    }

    [Fact]
    public void CalculateRoundScores_ZeroRoundScore_StillAddsBonus()
    {
        var wonPile = new List<Card>();
        for (int i = 0; i < 14; i++) wonPile.Add(new Card((byte)28, (sbyte)(i % 4)));

        var players = new[]
        {
            new Player(0)
            {
                RoundScore = 0,
                TotalScore = 500,
                WonPile = wonPile
            }
        };

        ScoringManager.CalculateRoundScores(players, GameMode.Advanced, playerCount: 2);

        Assert.Equal(640, players[0].TotalScore);
    }

    #endregion

    #region CheckForWinner Tests

    [Fact]
    public void CheckForWinner_NoPlayerReachedTarget_ReturnsMinusOne()
    {
        var players = new[]
        {
            new Player(0) { TotalScore = 1000 },
            new Player(1) { TotalScore = 1200 }
        };
        Assert.Equal(-1, ScoringManager.CheckForWinner(players, targetScore: 1500));
    }

    [Fact]
    public void CheckForWinner_FirstPlayerReachedTarget_ReturnsZero()
    {
        var players = new[]
        {
            new Player(0) { TotalScore = 1500 },
            new Player(1) { TotalScore = 1200 }
        };
        Assert.Equal(0, ScoringManager.CheckForWinner(players, targetScore: 1500));
    }

    [Fact]
    public void CheckForWinner_SecondPlayerReachedTarget_ReturnsOne()
    {
        var players = new[]
        {
            new Player(0) { TotalScore = 1200 },
            new Player(1) { TotalScore = 1600 }
        };
        Assert.Equal(1, ScoringManager.CheckForWinner(players, targetScore: 1500));
    }

    [Fact]
    public void CheckForWinner_MultiplePlayersReached_ReturnsFirstWinner()
    {
        var players = new[]
        {
            new Player(0) { TotalScore = 1500 },
            new Player(1) { TotalScore = 1600 },
            new Player(2) { TotalScore = 1400 },
            new Player(3) { TotalScore = 1550 }
        };
        Assert.Equal(0, ScoringManager.CheckForWinner(players, targetScore: 1500));
    }

    [Fact]
    public void CheckForWinner_FourPlayers_ReturnsCorrectWinner()
    {
        var players = new[]
        {
            new Player(0) { TotalScore = 800 },
            new Player(1) { TotalScore = 1000 },
            new Player(2) { TotalScore = 750 },
            new Player(3) { TotalScore = 950 }
        };
        Assert.Equal(1, ScoringManager.CheckForWinner(players, targetScore: 1000));
    }

    #endregion

    #region HasAnyPlayerReachedTarget Tests

    [Fact]
    public void HasAnyPlayerReachedTarget_NoPlayerReached_ReturnsFalse()
    {
        var players = new[]
        {
            new Player(0) { TotalScore = 1000 },
            new Player(1) { TotalScore = 1200 }
        };
        Assert.False(ScoringManager.HasAnyPlayerReachedTarget(players, targetScore: 1500));
    }

    [Fact]
    public void HasAnyPlayerReachedTarget_PlayerReached_ReturnsTrue()
    {
        var players = new[]
        {
            new Player(0) { TotalScore = 1500 },
            new Player(1) { TotalScore = 1200 }
        };
        Assert.True(ScoringManager.HasAnyPlayerReachedTarget(players, targetScore: 1500));
    }

    #endregion

    #region GetWinnerId Tests

    [Fact]
    public void GetWinnerId_SinglePlayer_ReturnsPlayerId()
    {
        var players = new[]
        {
            new Player(0) { TotalScore = 1000 }
        };
        Assert.Equal(0, ScoringManager.GetWinnerId(players));
    }

    [Fact]
    public void GetWinnerId_TwoPlayers_ReturnsHigherScore()
    {
        var players = new[]
        {
            new Player(0) { TotalScore = 1200 },
            new Player(1) { TotalScore = 1500 }
        };
        Assert.Equal(1, ScoringManager.GetWinnerId(players));
    }

    [Fact]
    public void GetWinnerId_Tie_ReturnsFirstPlayer()
    {
        var players = new[]
        {
            new Player(0) { TotalScore = 1500 },
            new Player(1) { TotalScore = 1500 }
        };
        Assert.Equal(0, ScoringManager.GetWinnerId(players));
    }

    [Fact]
    public void GetWinnerId_FourPlayers_ReturnsHighestScore()
    {
        var players = new[]
        {
            new Player(0) { TotalScore = 800 },
            new Player(1) { TotalScore = 1200 },
            new Player(2) { TotalScore = 950 },
            new Player(3) { TotalScore = 1100 }
        };
        Assert.Equal(1, ScoringManager.GetWinnerId(players));
    }

    [Fact]
    public void GetWinnerId_AllZeroScores_ReturnsFirstPlayer()
    {
        var players = new[]
        {
            new Player(0) { TotalScore = 0 },
            new Player(1) { TotalScore = 0 }
        };
        Assert.Equal(0, ScoringManager.GetWinnerId(players));
    }

    #endregion
}
