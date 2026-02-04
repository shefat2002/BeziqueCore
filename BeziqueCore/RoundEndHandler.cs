namespace BeziqueCore;

public static class RoundEndHandler
{
    public static int EndRound(Player[] players, int winningScoreThreshold)
    {
        int highestScore = int.MinValue;
        int winnerId = -1;

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].RoundScore > highestScore)
            {
                highestScore = players[i].RoundScore;
                winnerId = i;
            }
        }

        for (int i = 0; i < players.Length; i++)
        {
            players[i].TotalScore += players[i].RoundScore;
        }

        return winnerId;
    }

    public static bool CheckGameOver(Player[] players, int winningScoreThreshold)
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].TotalScore >= winningScoreThreshold)
            {
                return true;
            }
        }

        return false;
    }
}
