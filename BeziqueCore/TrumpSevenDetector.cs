namespace BeziqueCore;

public static class TrumpSevenDetector
{
    public static bool IsTrumpSeven(Card card, Suit trump)
    {
        return !card.IsJoker && card.Suit == trump && card.Rank == Rank.Seven;
    }

    public static int[] FindTrumpSevenPlayers(Card[] playedCards, Suit trump)
    {
        var players = new List<int>();

        for (int i = 0; i < playedCards.Length; i++)
        {
            if (IsTrumpSeven(playedCards[i], trump))
            {
                players.Add(i);
            }
        }

        return players.ToArray();
    }
}
