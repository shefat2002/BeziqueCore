namespace BeziqueCore;

public static class ScoringManager
{
    public static int CountAcesAndTens(List<Card> wonPile)
    {
        int count = 0;
        foreach (var card in wonPile)
        {
            if (card.Rank == Rank.Ace || card.Rank == Rank.Ten) count++;
        }
        return count;
    }

    public static int CalculateAdvancedBonus(int aceTenCount, byte playerCount)
    {
        int threshold = playerCount == 2 ? 14 : 8;
        if (aceTenCount < threshold) return 0;
        return aceTenCount * 10;
    }

    public static void CalculateRoundScores(Player[] players, GameMode mode, byte playerCount)
    {
        foreach (var player in players)
        {
            int roundScore = player.RoundScore;
            if (mode == GameMode.Advanced)
            {
                int aceTenCount = CountAcesAndTens(player.WonPile);
                int bonus = CalculateAdvancedBonus(aceTenCount, playerCount);
                player.TotalScore += roundScore + bonus;
            }
            else
            {
                player.TotalScore += roundScore;
            }
        }
    }

    public static int CheckForWinner(Player[] players, ushort targetScore)
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].TotalScore >= targetScore) return i;
        }
        return -1;
    }

    public static bool HasAnyPlayerReachedTarget(Player[] players, ushort targetScore)
    {
        return CheckForWinner(players, targetScore) != -1;
    }

    public static int GetWinnerId(Player[] players)
    {
        int winnerId = -1;
        int maxScore = -1;

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].TotalScore > maxScore)
            {
                maxScore = players[i].TotalScore;
                winnerId = i;
            }
        }

        return winnerId;
    }
}
