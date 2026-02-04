namespace BeziqueCore;

public static class NewTrickHandler
{
    public static void StartNewTrick(List<Card> playedCards, int winnerPlayerId, ref int currentTurnPlayer)
    {
        playedCards.Clear();
        currentTurnPlayer = winnerPlayerId;
    }
}
