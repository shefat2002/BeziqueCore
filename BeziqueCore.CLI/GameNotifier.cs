using BeziqueCore.Interfaces;
using BeziqueCore.Models;
using Spectre.Console;

namespace BeziqueCore.CLI
{
    public class GameNotifier : IGameStateNotifier
    {
        public event Action<Player>? OnPlayerTurn;
        public event Action<Player, Card>? OnCardPlayed;
        public event Action<Dictionary<Player, Card>>? OnTrickComplete;
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
            AnsiConsole.MarkupLine("[bold green]🎮 GAME STARTED! Good luck, players![/]");
        }

        public void NotifyTrumpDetermined(Suit trumpSuit, Card trumpCard)
        {
            OnTrumpDetermined?.Invoke(trumpSuit, trumpCard);
            var suitIcon = trumpSuit switch
            {
                Suit.Hearts => "♥",
                Suit.Diamonds => "♦",
                Suit.Clubs => "♣",
                Suit.Spades => "♠",
                _ => "?"
            };
            AnsiConsole.MarkupLine($"[bold yellow]🃏 Trump determined: {suitIcon} {trumpCard.Rank}[/]");
        }

        public void NotifyPlayerTurn(Player player)
        {
            OnPlayerTurn?.Invoke(player);
            AnsiConsole.MarkupLine($"[bold blue]👤 {player.Name}'s turn[/]");
        }

        public void NotifyCardPlayed(Player player, Card card)
        {
            OnCardPlayed?.Invoke(player, card);
            var cardStr = card.IsJoker ? "🃏 Joker" : $"{card.Rank} of {card.Suit}";
            AnsiConsole.MarkupLine($"[dim]{player.Name} played {cardStr}[/]");
        }

        public void NotifyTrickComplete(Dictionary<Player, Card> trick)
        {
            OnTrickComplete?.Invoke(trick);
            AnsiConsole.MarkupLine($"[dim]Trick complete! {trick.Count} cards played.[/]");
        }

        public void NotifyTrickWon(Player winner, Card[] cards, int points)
        {
            OnTrickWon?.Invoke(winner, cards, points);
            AnsiConsole.MarkupLine($"[bold green]🏆 {winner.Name} won the trick! (+{points} points)[/]");
        }

        public void NotifyMeldDeclared(Player player, Meld meld, int points)
        {
            OnMeldDeclared?.Invoke(player, meld, points);
            AnsiConsole.MarkupLine($"[bold magenta]✨ {player.Name} declared {meld.Type}! (+{points} points)[/]");
        }

        public void NotifySevenOfTrumpSwitched(Player player)
        {
            OnSevenOfTrumpSwitched?.Invoke(player);
            AnsiConsole.MarkupLine($"[bold cyan]🔄 {player.Name} switched 7 of Trump! (+10 points)[/]");
        }

        public void NotifyRoundEnded(Dictionary<Player, int> scores)
        {
            OnRoundEnded?.Invoke(scores);
            AnsiConsole.MarkupLine("[bold yellow]📊 ROUND ENDED[/]");

            var table = new Table();
            table.Border(TableBorder.Simple);
            table.AddColumn("Player");
            table.AddColumn("Score");

            foreach (var score in scores.OrderByDescending(s => s.Value))
            {
                table.AddRow(score.Key.Name, score.Value.ToString());
            }
            AnsiConsole.Write(table);
        }

        public void NotifyGameOver(Player winner)
        {
            OnGameOver?.Invoke(winner);
            AnsiConsole.MarkupLine($"[bold green]🎉 GAME OVER! {winner.Name} WINS![/]");
        }

        public void NotifyPlayerTimeout(Player player)
        {
            OnPlayerTimeout?.Invoke(player);
            AnsiConsole.MarkupLine($"[red]⏰ {player.Name} timed out! (-10 points)[/]");
        }

        public void NotifyLastNineCardsStarted()
        {
            OnLastNineCardsStarted?.Invoke();
            AnsiConsole.MarkupLine("[bold red]⚠️ LAST 9 CARDS PHASE! Strict rules apply![/]");
        }

        public void NotifyCardsDealt(Dictionary<Player, List<Card>> hands)
        {
            OnCardsDealt?.Invoke(hands);
        }
    }
}
