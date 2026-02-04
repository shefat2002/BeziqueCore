namespace BeziqueCore;

public static class AceTenCounter
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
}
