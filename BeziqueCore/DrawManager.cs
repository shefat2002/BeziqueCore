namespace BeziqueCore;

public static class DrawManager
{
    public static bool ExecuteDraw(Player[] players, int winnerId, Card trumpCard, Stack<Card> drawDeck, byte playerCount)
    {
        bool isFinalDraw = PhaseTransitionDetector.ShouldTransitionToPhase2(drawDeck.Count, playerCount);

        if (isFinalDraw)
        {
            FinalDrawCoordinator.ExecuteFinalDraw(players, winnerId, trumpCard, drawDeck);
            TableClearer.ReturnAllTableCardsToHand(players);
            return true;
        }

        var winner = players.First(p => p.PlayerID == winnerId);
        if (drawDeck.Count > 0)
        {
            winner.Hand.Add(drawDeck.Pop());
        }

        foreach (var player in players.Where(p => p.PlayerID != winnerId))
        {
            if (drawDeck.Count > 0)
            {
                player.Hand.Add(drawDeck.Pop());
            }
        }

        return false;
    }
}
