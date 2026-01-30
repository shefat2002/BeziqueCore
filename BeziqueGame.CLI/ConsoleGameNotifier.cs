using Spectre.Console;
using System;
using System.Collections.Generic;
using BeziqueCore.Models;
using BeziqueCore.Interfaces;

namespace BeziqueGame.CLI
{
    /// <summary>
    /// Console-based implementation of IGameStateNotifier for CLI game feedback.
    /// </summary>
    public class ConsoleGameNotifier : IGameStateNotifier
    {
        public void NotifyGameStarted()
        {
            AnsiConsole.MarkupLine("[bold green]Game Started![/]\n");
        }

        public void NotifyTrumpDetermined(Suit trumpSuit, Card trumpCard)
        {
            AnsiConsole.MarkupLine($"[bold yellow]Trump is {trumpSuit} ({trumpCard.Rank})[/]\n");
        }

        public void NotifyPlayerTurn(Player player)
        {
            AnsiConsole.MarkupLine($"\n[bold]{player.Name}'s turn[/]\n");
        }

        public void NotifyCardPlayed(Player player, Card card)
        {
            AnsiConsole.MarkupLine($"[grey]{player.Name} played {card.Rank} of {card.Suit}[/]");
        }

        public void NotifyTrickComplete(Dictionary<Player, Card> trick)
        {
            AnsiConsole.MarkupLine("\n[grey]Trick complete[/]");
        }

        public void NotifyTrickWon(Player winner, Card[] cards, int points)
        {
            AnsiConsole.MarkupLine($"\n[bold yellow]{winner.Name} wins the trick! (+{points} points)[/]\n");
        }

        public void NotifyMeldDeclared(Player player, Meld meld, int points)
        {
            AnsiConsole.MarkupLine($"[bold green]{player.Name} declared {meld.Type} for {points} points![/]");
        }

        public void NotifySevenOfTrumpSwitched(Player player)
        {
            AnsiConsole.MarkupLine($"[bold magenta]{player.Name} switched 7 of Trump![/]");
        }

        public void NotifySevenOfTrumpPlayed(Player player)
        {
            AnsiConsole.MarkupLine($"[bold magenta]{player.Name} played 7 of Trump! (+10 points)[/]");
        }

        public void NotifyTrumpCardTaken(Player player, Card trumpCard)
        {
            AnsiConsole.MarkupLine($"[cyan]{player.Name} took the trump card[/]");
        }

        public void NotifyLastTrickBonus(Player winner, int points)
        {
            AnsiConsole.MarkupLine($"[bold yellow]{winner.Name} gets last trick bonus! (+{points} points)[/]");
        }

        public void NotifyRoundEnded(Dictionary<Player, int> scores)
        {
            AnsiConsole.MarkupLine("\n[bold red]=== ROUND END ===[/]\n");
            AnsiConsole.MarkupLine("[bold]Scores:[/]");
            foreach (var kvp in scores)
            {
                AnsiConsole.MarkupLine($"  {kvp.Key.Name}: {kvp.Value} points");
            }
        }

        public void NotifyGameOver(Player winner)
        {
            AnsiConsole.MarkupLine($"\n[bold red]=== GAME OVER ===[/]\n");
            AnsiConsole.MarkupLine($"[bold green]{winner.Name} WINS![/]\n");
        }

        public void NotifyPlayerTimeout(Player player)
        {
            AnsiConsole.MarkupLine($"[red]{player.Name} timed out![/]");
        }

        public void NotifyLastNineCardsStarted()
        {
            AnsiConsole.MarkupLine("\n[yellow]=== LAST 9 CARDS PHASE ===[/]\n");
        }

        public void NotifyCardsDealt(Dictionary<Player, List<Card>> hands)
        {
            AnsiConsole.MarkupLine("[yellow]Cards dealt to all players[/]\n");
        }

        public void NotifyAcesAndTensBonus(Player player, int bonusPoints)
        {
            AnsiConsole.MarkupLine($"[bold magenta]{player.Name} gets Aces & Tens bonus! (+{bonusPoints} points)[/]");
        }

        // Additional helper methods not in interface
        public void NotifyGameInitialized()
        {
            AnsiConsole.MarkupLine("[bold green]Game Initialized![/]\n");
        }

        public void NotifyModeSelected(GameMode mode)
        {
            AnsiConsole.MarkupLine($"[bold cyan]Mode: {mode}[/]\n");
        }

        public void NotifyMeldSkipped(Player player)
        {
            AnsiConsole.MarkupLine($"[grey]{player.Name} skipped meld[/]");
        }

        public void NotifyTrickResolved(Player winner, int points)
        {
            // Legacy method for compatibility
            NotifyTrickWon(winner, Array.Empty<Card>(), points);
        }
    }
}
