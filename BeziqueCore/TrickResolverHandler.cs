namespace BeziqueCore;

public static class TrickResolverHandler
{
    public static int ResolveTrick(List<Card> playedCards, Player[] players, int[] playerIndices, Suit trump, bool isFinalTrick)
    {
        int winnerIndex = TrickEvaluator.GetWinner(playedCards.ToArray(), trump);
        int winnerPlayerId = playerIndices[winnerIndex];

        var trumpSevenIndices = TrumpSevenDetector.FindTrumpSevenPlayers(playedCards.ToArray(), trump);
        foreach (var cardIndex in trumpSevenIndices)
        {
            players[playerIndices[cardIndex]].RoundScore += 10;
        }

        if (isFinalTrick)
        {
            players[winnerPlayerId].RoundScore += 10;
        }

        foreach (var card in playedCards)
        {
            players[winnerPlayerId].WonPile.Add(card);
        }

        return winnerPlayerId;
    }
}
