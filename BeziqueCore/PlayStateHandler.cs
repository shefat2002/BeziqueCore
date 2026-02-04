namespace BeziqueCore;

public static class PlayStateHandler
{
    public static bool ValidateAndPlayCardPhase1(Player player, Card card, List<Card> playedCards, int playerIndex)
    {
        if (!PlayCardValidator.TryPlayCard(player, card, out var playedCard)) return false;

        playedCards.Add(playedCard);
        return true;
    }
}
