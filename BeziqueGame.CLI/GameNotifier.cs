using Spectre.Console;
using BeziqueCore.Models;
using BeziqueCore.Interfaces;

namespace BeziqueGame.CLI
{
    public class GameNotifier : IGameStateNotifier
    {
        public void NotifyGameStarted()
        {
            AnsiConsole.MarkupLine("[bold green]🎮 Game Started![/]");
        }

        public void NotifyCardsDealt(Dictionary<Player, List<Card>> hands)
        {
            foreach (var kvp in hands)
            {
                AnsiConsole.MarkupLine($"[dim]Dealt {kvp.Value.Count} cards to {kvp.Key.Name}[/]");
            }
        }

        public void NotifyTrumpDetermined(Suit trumpSuit, Card trumpCard)
        {
            AnsiConsole.MarkupLine($"[bold yellow]🃏 Trump: {trumpSuit}[/] ([dim]{trumpCard.Rank} of {trumpCard.Suit}[/])");
        }

        public void NotifyPlayerTurn(Player player)
        {
            // Optionally show whose turn it is
        }

        public void NotifyCardPlayed(Player player, Card card)
        {
            // Handled by game controller
        }

        public void NotifyTrickComplete(Dictionary<Player, Card> trick)
        {
            // Handled by game controller
        }

        public void NotifyTrickWon(Player winner, Card[] cards, int points)
        {
            // Handled by game controller
        }

        public void NotifyMeldDeclared(Player player, Meld meld, int points)
        {
            // Handled by game controller
        }

        public void NotifySevenOfTrumpSwitched(Player player)
        {
            AnsiConsole.MarkupLine($"[bold cyan]🔄 {player.Name} switched 7 of Trump with trump card![/]");
        }

        public void NotifySevenOfTrumpPlayed(Player player)
        {
            AnsiConsole.MarkupLine($"[bold green]✨ {player.Name} played 7 of Trump! (+10 points)[/]");
        }

        public void NotifyTrumpCardTaken(Player player, Card trumpCard)
        {
            AnsiConsole.MarkupLine($"[dim]{player.Name} took the trump card[/]");
        }

        public void NotifyLastTrickBonus(Player winner, int points)
        {
            AnsiConsole.MarkupLine($"[bold green]🏆 {winner.Name} gets last trick bonus! (+{points} points)[/]");
        }

        public void NotifyRoundEnded(Dictionary<Player, int> scores)
        {
            // Handled by game controller
        }

        public void NotifyGameOver(Player winner)
        {
            // Handled by game controller
        }

        public void NotifyPlayerTimeout(Player player)
        {
            AnsiConsole.MarkupLine($"[bold red]⏱️ {player.Name} timed out![/]");
        }

        public void NotifyLastNineCardsStarted()
        {
            AnsiConsole.MarkupLine("[bold yellow]⚠️ Last 9 cards phase started! Strict rules apply.[/]");
        }
    }
}
