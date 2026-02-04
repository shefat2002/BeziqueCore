namespace BeziqueCore;

public static class L9PlayStateHandler
{
    public static bool ValidateAndPlayCardPhase2(Player player, Card card, List<Card> playedCards, int playerIndex, Suit leadSuit, Card? currentWinner, Suit trump)
    {
        if (!Phase2MoveValidator.IsLegalMove(player.Hand, card, leadSuit, currentWinner, trump)) return false;

        if (!PlayCardValidator.TryRemoveCard(player.Hand, card)) return false;

        playedCards.Add(card);
        return true;
    }
}
