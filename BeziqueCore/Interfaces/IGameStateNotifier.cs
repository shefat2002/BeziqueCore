using BeziqueCore.Models;

namespace BeziqueCore.Interfaces
{
    public interface IGameStateNotifier
    {
        void NotifyGameStarted();
        void NotifyTrumpDetermined(Suit trumpSuit, Card trumpCard);
        void NotifyPlayerTurn(Player player);
        void NotifyCardPlayed(Player player, Card card);
        void NotifyTrickComplete(Dictionary<Player, Card> trick);
        void NotifyTrickWon(Player winner, Card[] cards, int points);
        void NotifyMeldDeclared(Player player, Meld meld, int points);
        void NotifySevenOfTrumpSwitched(Player player);
        void NotifySevenOfTrumpPlayed(Player player);
        void NotifyTrumpCardTaken(Player player, Card trumpCard);
        void NotifyLastTrickBonus(Player winner, int points);
        void NotifyRoundEnded(Dictionary<Player, int> scores);
        void NotifyGameOver(Player winner);
        void NotifyPlayerTimeout(Player player);
        void NotifyLastNineCardsStarted();
        void NotifyCardsDealt(Dictionary<Player, List<Card>> hands);
    }
}
