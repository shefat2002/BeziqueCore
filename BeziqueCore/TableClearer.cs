namespace BeziqueCore;

public static class TableClearer
{
    public static void ReturnAllTableCardsToHand(Player[] players)
    {
        foreach (var player in players)
        {
            if (player.TableCards.Count > 0)
            {
                player.Hand.AddRange(player.TableCards);
                player.TableCards.Clear();
            }
        }
    }
}
