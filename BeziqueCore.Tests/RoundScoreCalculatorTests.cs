using BeziqueCore;

public class RoundScoreCalculatorTests
{
    [Fact]
    public void CalculateRoundScores_StandardMode_AddsOnlyRoundScore()
    {
        var players = new[]
        {
            new Player(0) { RoundScore = 100, TotalScore = 500, WonPile = new List<Card>() },
            new Player(1) { RoundScore = 150, TotalScore = 600, WonPile = new List<Card>() }
        };

        RoundScoreCalculator.CalculateRoundScores(players, GameMode.Standard, playerCount: 2);

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

        RoundScoreCalculator.CalculateRoundScores(players, GameMode.Advanced, playerCount: 2);

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

        RoundScoreCalculator.CalculateRoundScores(players, GameMode.Advanced, playerCount: 2);

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

        RoundScoreCalculator.CalculateRoundScores(players, GameMode.Advanced, playerCount: 4);

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

        RoundScoreCalculator.CalculateRoundScores(players, GameMode.Advanced, playerCount: 2);

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

        RoundScoreCalculator.CalculateRoundScores(players, GameMode.Advanced, playerCount: 2);

        Assert.Equal(640, players[0].TotalScore);
    }
}
