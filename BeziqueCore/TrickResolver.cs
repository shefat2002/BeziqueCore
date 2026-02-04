namespace BeziqueCore;

public static class TrickResolver
{
    public static int ResolveTrick(Card[] playedCards, Player[] players, Suit trump)
    {
        if (playedCards.Length == 0) return -1;

        int winnerIndex = TrickEvaluator.GetWinner(playedCards, trump);
        players[winnerIndex].WonPile.AddRange(playedCards);
        players[winnerIndex].WonPile.TrimExcess();

        int[] trumpSevenPlayers = TrumpSevenDetector.FindTrumpSevenPlayers(playedCards, trump);
        for (int i = 0; i < trumpSevenPlayers.Length; i++)
        {
            players[trumpSevenPlayers[i]].RoundScore += 10;
        }

        return winnerIndex;
    }

    public static int ResolveTrick(IReadOnlyList<Card> playedCards, Player[] players, Suit trump)
    {
        if (playedCards.Count == 0) return -1;

        int winnerIndex = TrickEvaluator.GetWinner(playedCards, trump);

        for (int i = 0; i < playedCards.Count; i++)
        {
            players[winnerIndex].WonPile.Add(playedCards[i]);
        }
        players[winnerIndex].WonPile.TrimExcess();

        for (int i = 0; i < playedCards.Count; i++)
        {
            if (TrumpSevenDetector.IsTrumpSeven(playedCards[i], trump))
            {
                players[i].RoundScore += 10;
            }
        }

        return winnerIndex;
    }
}
