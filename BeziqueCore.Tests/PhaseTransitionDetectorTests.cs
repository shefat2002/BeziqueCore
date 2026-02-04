using BeziqueCore;

public class PhaseTransitionDetectorTests
{
    [Fact]
    public void ShouldTransitionToPhase2_ReturnsFalse_WhenDeckHasMoreCardsThanPlayers()
    {
        Assert.False(PhaseTransitionDetector.ShouldTransitionToPhase2(deckCount: 10, playerCount: 2));
    }

    [Fact]
    public void ShouldTransitionToPhase2_ReturnsFalse_WhenDeckHasFewerCardsThanPlayers()
    {
        Assert.False(PhaseTransitionDetector.ShouldTransitionToPhase2(deckCount: 1, playerCount: 2));
    }

    [Fact]
    public void ShouldTransitionToPhase2_ReturnsTrue_WhenDeckEqualsPlayers_TwoPlayers()
    {
        Assert.True(PhaseTransitionDetector.ShouldTransitionToPhase2(deckCount: 2, playerCount: 2));
    }

    [Fact]
    public void ShouldTransitionToPhase2_ReturnsTrue_WhenDeckEqualsPlayers_FourPlayers()
    {
        Assert.True(PhaseTransitionDetector.ShouldTransitionToPhase2(deckCount: 4, playerCount: 4));
    }

    [Fact]
    public void ShouldTransitionToPhase2_ReturnsFalse_WhenDeckIsEmpty()
    {
        Assert.False(PhaseTransitionDetector.ShouldTransitionToPhase2(deckCount: 0, playerCount: 2));
    }
}
