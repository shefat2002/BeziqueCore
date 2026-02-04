namespace BeziqueCore;

public static class TurnManager
{
    public static int AdvanceTurn(int currentPlayerId, byte playerCount) =>
        (currentPlayerId + 1) % playerCount;

    public static int SetFirstTurn(int winnerId) => winnerId;
}
