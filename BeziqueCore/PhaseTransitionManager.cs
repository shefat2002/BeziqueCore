namespace BeziqueCore;

public static class PhaseTransitionManager
{
    public static bool ShouldTransitionToPhase2(int deckCount, int playerCount) =>
        deckCount == playerCount;

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

    public static bool ExecuteDraw(Player[] players, int winnerId, Card trumpCard, Stack<Card> drawDeck, byte playerCount)
    {
        bool isFinalDraw = ShouldTransitionToPhase2(drawDeck.Count, playerCount);

        if (isFinalDraw)
        {
            ExecuteFinalDraw(players, winnerId, trumpCard, drawDeck);
            ReturnAllTableCardsToHand(players);
            return true;
        }

        // ALL players draw 1 card each after a trick ends
        // Winner draws first, then other players in order
        foreach (var player in players)
        {
            if (drawDeck.Count > 0)
            {
                player.Hand.Add(drawDeck.Pop());
            }
        }

        return false;
    }
}
