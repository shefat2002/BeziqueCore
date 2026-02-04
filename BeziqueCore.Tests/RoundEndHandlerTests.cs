using BeziqueCore;

public class RoundEndHandlerTests
{
    [Fact]
    public void EndRound_ReturnsHighestScoringPlayerId()
    {
        var players = new[]
        {
            new Player(0) { RoundScore = 100 },
            new Player(1) { RoundScore = 150 },
            new Player(2) { RoundScore = 120 }
        };

        int winnerId = RoundEndHandler.EndRound(players, 1000);

        Assert.Equal(1, winnerId);
    }

    [Fact]
    public void EndRound_AddsRoundScoresToTotalScores()
    {
        var players = new[]
        {
            new Player(0) { RoundScore = 100, TotalScore = 500 },
            new Player(1) { RoundScore = 150, TotalScore = 600 }
        };

        RoundEndHandler.EndRound(players, 1000);

        Assert.Equal(600, players[0].TotalScore);
        Assert.Equal(750, players[1].TotalScore);
    }

    [Fact]
    public void EndRound_Tie_ReturnsFirstHighestPlayerId()
    {
        var players = new[]
        {
            new Player(0) { RoundScore = 150 },
            new Player(1) { RoundScore = 150 },
            new Player(2) { RoundScore = 100 }
        };

        int winnerId = RoundEndHandler.EndRound(players, 1000);

        Assert.Equal(0, winnerId);
    }

    [Fact]
    public void EndRound_SinglePlayer_ReturnsThatPlayerId()
    {
        var players = new[]
        {
            new Player(0) { RoundScore = 100 }
        };

        int winnerId = RoundEndHandler.EndRound(players, 1000);

        Assert.Equal(0, winnerId);
    }

    [Fact]
    public void EndRound_FourPlayers_ReturnsCorrectWinner()
    {
        var players = new[]
        {
            new Player(0) { RoundScore = 100 },
            new Player(1) { RoundScore = 200 },
            new Player(2) { RoundScore = 150 },
            new Player(3) { RoundScore = 180 }
        };

        int winnerId = RoundEndHandler.EndRound(players, 1000);

        Assert.Equal(1, winnerId);
    }

    [Fact]
    public void CheckGameOver_PlayerAtThreshold_ReturnsTrue()
    {
        var players = new[]
        {
            new Player(0) { TotalScore = 1000 },
            new Player(1) { TotalScore = 500 }
        };

        bool isGameOver = RoundEndHandler.CheckGameOver(players, 1000);

        Assert.True(isGameOver);
    }

    [Fact]
    public void CheckGameOver_PlayerAboveThreshold_ReturnsTrue()
    {
        var players = new[]
        {
            new Player(0) { TotalScore = 1200 },
            new Player(1) { TotalScore = 500 }
        };

        bool isGameOver = RoundEndHandler.CheckGameOver(players, 1000);

        Assert.True(isGameOver);
    }

    [Fact]
    public void CheckGameOver_NoPlayerAtThreshold_ReturnsFalse()
    {
        var players = new[]
        {
            new Player(0) { TotalScore = 999 },
            new Player(1) { TotalScore = 500 }
        };

        bool isGameOver = RoundEndHandler.CheckGameOver(players, 1000);

        Assert.False(isGameOver);
    }

    [Fact]
    public void CheckGameOver_MultiplePlayersAtThreshold_ReturnsTrue()
    {
        var players = new[]
        {
            new Player(0) { TotalScore = 1000 },
            new Player(1) { TotalScore = 1000 }
        };

        bool isGameOver = RoundEndHandler.CheckGameOver(players, 1000);

        Assert.True(isGameOver);
    }

    [Fact]
    public void CheckGameOver_ZeroThreshold_AllPlayersQualify()
    {
        var players = new[]
        {
            new Player(0) { TotalScore = 0 },
            new Player(1) { TotalScore = 0 }
        };

        bool isGameOver = RoundEndHandler.CheckGameOver(players, 0);

        Assert.True(isGameOver);
    }
}
