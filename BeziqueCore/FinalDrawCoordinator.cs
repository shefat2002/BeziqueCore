namespace BeziqueCore;

public static class FinalDrawCoordinator
{
    public static void ExecuteFinalDraw(Player[] players, int winnerId, Card trumpCard, Stack<Card> drawDeck)
    {
        var winner = players.First(p => p.PlayerID == winnerId);
        var nonWinners = players.Where(p => p.PlayerID != winnerId).ToArray();

        var hiddenCard = drawDeck.Pop();
        winner.Hand.Add(hiddenCard);

        foreach (var player in nonWinners)
        {
            player.Hand.Add(trumpCard);
        }
    }
}
