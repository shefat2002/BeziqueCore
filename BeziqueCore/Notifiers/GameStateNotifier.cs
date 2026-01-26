using BeziqueCore.Interfaces;
using BeziqueCore.Models;

namespace BeziqueCore.Notifiers
{
    public class GameStateNotifier : IGameStateNotifier
    {
        public event Action<Player>? OnPlayerTurn;
        public event Action<Player, Card>? OnCardPlayed;
        public event Action<Player, Card[], int>? OnTrickWon;
        public event Action<Player, Meld, int>? OnMeldDeclared;
        public event Action<Player>? OnSevenOfTrumpSwitched;
        public event Action<Dictionary<Player, int>>? OnRoundEnded;
        public event Action<Player>? OnGameOver;
        public event Action<Player>? OnPlayerTimeout;
        public event Action? OnLastNineCardsStarted;
        public event Action<Suit, Card>? OnTrumpDetermined;
        public event Action? OnGameStarted;
        public event Action<Dictionary<Player, List<Card>>>? OnCardsDealt;

        public void NotifyGameStarted()
        {
            OnGameStarted?.Invoke();
            Console.WriteLine("Game started!");
        }

        public void NotifyTrumpDetermined(Suit trumpSuit, Card trumpCard)
        {
            OnTrumpDetermined?.Invoke(trumpSuit, trumpCard);
            Console.WriteLine($"Trump suit determined: {trumpSuit}");
        }

        public void NotifyPlayerTurn(Player player)
        {
            OnPlayerTurn?.Invoke(player);
            Console.WriteLine($"It's {player.Name}'s turn");
        }

        public void NotifyCardPlayed(Player player, Card card)
        {
            OnCardPlayed?.Invoke(player, card);
            string cardStr = card.IsJoker ? "Joker" : $"{card.Rank} of {card.Suit}";
            Console.WriteLine($"{player.Name} played {cardStr}");
        }

        public void NotifyTrickWon(Player winner, Card[] cards, int points)
        {
            OnTrickWon?.Invoke(winner, cards, points);
            Console.WriteLine($"{winner.Name} won the trick ({points} points)");
        }

        public void NotifyMeldDeclared(Player player, Meld meld, int points)
        {
            OnMeldDeclared?.Invoke(player, meld, points);
            Console.WriteLine($"{player.Name} declared {meld.Type} ({points} points)");
        }

        public void NotifySevenOfTrumpSwitched(Player player)
        {
            OnSevenOfTrumpSwitched?.Invoke(player);
            Console.WriteLine($"{player.Name} switched 7 of trump (+10 points)");
        }

        public void NotifyRoundEnded(Dictionary<Player, int> scores)
        {
            OnRoundEnded?.Invoke(scores);
            Console.WriteLine("Round ended!");
            foreach (var score in scores)
            {
                Console.WriteLine($"  {score.Key.Name}: {score.Value} points");
            }
        }

        public void NotifyGameOver(Player winner)
        {
            OnGameOver?.Invoke(winner);
            Console.WriteLine($"Game over! {winner.Name} wins!");
        }

        public void NotifyPlayerTimeout(Player player)
        {
            OnPlayerTimeout?.Invoke(player);
            Console.WriteLine($"{player.Name} timed out (-10 points)");
        }

        public void NotifyLastNineCardsStarted()
        {
            OnLastNineCardsStarted?.Invoke();
            Console.WriteLine("Last 9 cards phase started!");
        }

        public void NotifyCardsDealt(Dictionary<Player, List<Card>> hands)
        {
            OnCardsDealt?.Invoke(hands);
            Console.WriteLine("Cards dealt to all players");
        }
    }
}
