namespace BeziqueCore;

public static class GameWinChecker
{
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
