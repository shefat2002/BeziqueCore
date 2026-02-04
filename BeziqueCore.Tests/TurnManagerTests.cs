using BeziqueCore;

public class TurnManagerTests
{
    [Fact]
    public void AdvanceTurn_TwoPlayers_ReturnsOtherPlayer()
    {
        Assert.Equal(1, TurnManager.AdvanceTurn(0, playerCount: 2));
        Assert.Equal(0, TurnManager.AdvanceTurn(1, playerCount: 2));
    }

    [Fact]
    public void AdvanceTurn_FourPlayers_CyclesCorrectly()
    {
        Assert.Equal(1, TurnManager.AdvanceTurn(0, playerCount: 4));
        Assert.Equal(2, TurnManager.AdvanceTurn(1, playerCount: 4));
        Assert.Equal(3, TurnManager.AdvanceTurn(2, playerCount: 4));
        Assert.Equal(0, TurnManager.AdvanceTurn(3, playerCount: 4));
    }

    [Fact]
    public void SetFirstTurn_ReturnsWinnerId()
    {
        Assert.Equal(0, TurnManager.SetFirstTurn(0));
        Assert.Equal(1, TurnManager.SetFirstTurn(1));
        Assert.Equal(3, TurnManager.SetFirstTurn(3));
    }
}
