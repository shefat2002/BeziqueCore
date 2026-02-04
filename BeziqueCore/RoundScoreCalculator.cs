namespace BeziqueCore;

public static class RoundScoreCalculator
{
    public static void CalculateRoundScores(Player[] players, GameMode mode, byte playerCount)
    {
        foreach (var player in players)
        {
            int roundScore = player.RoundScore;
            if (mode == GameMode.Advanced)
            {
                int aceTenCount = AceTenCounter.CountAcesAndTens(player.WonPile);
                int bonus = AdvancedBonusCalculator.CalculateBonus(aceTenCount, playerCount);
                player.TotalScore += roundScore + bonus;
            }
            else
            {
                player.TotalScore += roundScore;
            }
        }
    }
}
