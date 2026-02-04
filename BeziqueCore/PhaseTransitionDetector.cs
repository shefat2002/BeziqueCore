namespace BeziqueCore;

public static class PhaseTransitionDetector
{
    public static bool ShouldTransitionToPhase2(int deckCount, int playerCount) =>
        deckCount == playerCount;
}
