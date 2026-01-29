using Spectre.Console;
using BeziqueCore.Models;
using BeziqueCore.Interfaces;
using BeziqueCore.Helpers;

namespace BeziqueGame.CLI
{
    public class GameUI
    {
        private readonly IGameState _gameState;
        private readonly IDeckOperations _deckOps;
        private readonly IMeldValidator _meldValidator;

        public GameUI(IGameState gameState, IDeckOperations deckOps, IMeldValidator meldValidator)
        {
            _gameState = gameState;
            _deckOps = deckOps;
            _meldValidator = meldValidator;
        }

        public void DisplayGameInfo()
        {
            var info = new Table().Border(TableBorder.None).Width(Console.WindowWidth - 4);
            info.AddColumn(new TableColumn(Markup.Escape("Trump:")).Width(20));
            info.AddColumn(new TableColumn(Markup.Escape("Deck:")).Width(20));
            info.AddColumn(new TableColumn(Markup.Escape("Phase:")).Width(30));

            var trumpDisplay = _gameState.TrumpSuit switch
            {
                Suit.Hearts => "[red]♥[/]",
                Suit.Diamonds => "[red]♦[/]",
                Suit.Clubs => "[blue]♣[/]",
                Suit.Spades => "[blue]♠[/]",
                _ => "?"
            };

            var deckCount = _deckOps.GetRemainingCardCount();
            var phase = IsLastNineCardsPhase() ? "[red]Last 9 Cards[/]" : "[green]Normal[/]";

            info.AddRow(trumpDisplay, $"{deckCount} cards", phase);
            AnsiConsole.Write(info);
        }

        public void DisplayPlayerHand(Player player)
        {
            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn($"[bold]{player.Name}'s Hand[/]");

            for (int i = 0; i < player.Hand.Count; i++)
            {
                table.AddRow($"[dim]{i + 1}.[/] {GetCardDisplay(player.Hand[i])}");
            }

            table.Expand();
            AnsiConsole.Write(table);
        }

        public void DisplayAllHands()
        {
            foreach (var player in _gameState.Players)
            {
                AnsiConsole.MarkupLine($"\n[bold]{player.Name}'s Hand:[/]");
                for (int i = 0; i < player.Hand.Count; i++)
                {
                    AnsiConsole.MarkupLine($"  {i + 1}. {GetCardDisplay(player.Hand[i])}");
                }
            }
        }

        public void DisplayGameStatus()
        {
            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn("[bold]Player[/]");
            table.AddColumn("[bold]Score[/]");
            table.AddColumn("[bold]Cards[/]");
            table.AddColumn("[bold]Dealer[/]");

            foreach (var player in _gameState.Players)
            {
                table.AddRow(
                    player.Name,
                    player.Score.ToString(),
                    player.Hand.Count.ToString(),
                    player.IsDealer ? "[yellow]Yes[/]" : "[dim]No[/]"
                );
            }

            AnsiConsole.Write(table);

            AnsiConsole.MarkupLine($"\n[bold]Trump:[/] {GetSuitDisplay(_gameState.TrumpSuit)}");
            AnsiConsole.MarkupLine($"[bold]Deck:[/] {_deckOps.GetRemainingCardCount()} cards");

            if (_gameState.CurrentTrick.Count > 0)
            {
                AnsiConsole.MarkupLine("\n[bold]Current Trick:[/]");
                foreach (var trickCard in _gameState.CurrentTrick)
                {
                    AnsiConsole.MarkupLine($"  {trickCard.Key.Name}: {GetCardDisplay(trickCard.Value)}");
                }
            }
        }

        public void DisplayTrumpInfo()
        {
            var trump = _gameState.TrumpSuit;
            var flipped = _gameState.TrumpCard;

            AnsiConsole.MarkupLine($"[bold yellow]Trump Suit:[/] {GetSuitDisplay(trump)}");
            if (flipped != null)
            {
                AnsiConsole.MarkupLine($"[bold]Flipped Card:[/] {GetCardDisplay(flipped)}");
            }
            AnsiConsole.MarkupLine("\n[dim]Cards in this suit are trumps and beat all other suits.[/]");
        }

        public void DisplayTrickFormation()
        {
            if (_gameState.CurrentTrick.Count == 0)
            {
                AnsiConsole.MarkupLine("\n[dim]No cards played yet this trick.[/]");
                return;
            }

            var table = new Table().Border(TableBorder.Rounded).Centered();
            table.AddColumn(new TableColumn("[bold dim]Current Trick[/]").Centered());

            foreach (var trickCard in _gameState.CurrentTrick)
            {
                var playerName = trickCard.Key.Name;
                table.AddRow($"[cyan]{playerName}[/]");
                table.AddRow(new Panel(GetCardDisplay(trickCard.Value))
                {
                    Border = BoxBorder.Rounded,
                    Padding = new Padding(1, 1, 1, 1)
                });
                table.AddEmptyRow();
            }

            AnsiConsole.Write(table);
        }

        public void DisplayScores()
        {
            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn("[bold]Player[/]");
            table.AddColumn("[bold]Score[/]");

            foreach (var player in _gameState.Players.OrderByDescending(p => p.Score))
            {
                table.AddRow(player.Name, player.Score.ToString());
            }

            AnsiConsole.Write(table);
        }

        public string GetCardDisplay(Card card)
        {
            if (card.IsJoker) return "[bold magenta]🃏 Joker[/]";

            var suitColor = card.Suit == Suit.Hearts || card.Suit == Suit.Diamonds ? "red" : "blue";
            var suitSymbol = card.Suit switch
            {
                Suit.Hearts => "♥",
                Suit.Diamonds => "♦",
                Suit.Clubs => "♣",
                Suit.Spades => "♠",
                _ => "?"
            };

            return $"[{suitColor}]{suitSymbol}{card.Rank}[/]";
        }

        public string GetSuitDisplay(Suit suit)
        {
            return suit switch
            {
                Suit.Hearts => "[red]♥[/]",
                Suit.Diamonds => "[red]♦[/]",
                Suit.Clubs => "[blue]♣[/]",
                Suit.Spades => "[blue]♠[/]",
                _ => "?"
            };
        }

        public List<Card> GetValidCardsForL9(Player player)
        {
            var currentTrick = _gameState.CurrentTrick;

            if (currentTrick.Count == 0)
            {
                return player.Hand.ToList();
            }

            var leadCard = currentTrick.Values.First();
            var leadSuit = leadCard.Suit;
            var leadRank = leadCard.Rank;

            // Find cards following suit with higher rank
            var sameSuitHigher = player.Hand
                .Where(c => c.Suit == leadSuit && GetRankValue(c.Rank) > GetRankValue(leadRank))
                .ToList();

            if (sameSuitHigher.Any())
                return sameSuitHigher;

            // No higher same suit - can play any same suit card
            var sameSuit = player.Hand.Where(c => c.Suit == leadSuit).ToList();
            if (sameSuit.Any())
                return sameSuit;

            // No same suit - can play any card
            return player.Hand.ToList();
        }

        private int GetRankValue(Rank rank)
        {
            return rank switch
            {
                Rank.Ace => 14,
                Rank.Ten => 13,
                Rank.King => 12,
                Rank.Queen => 11,
                Rank.Jack => 10,
                Rank.Nine => 9,
                Rank.Eight => 8,
                Rank.Seven => 7,
                _ => 0
            };
        }

        public List<PossibleMeld> GetAvailableMelds(Player player)
        {
            var melds = new List<PossibleMeld>();
            var trumpSuit = _gameState.TrumpSuit;

            // Find all possible melds from hand using MeldHelper
            var possibleMelds = MeldHelper.FindAllPossibleMelds(player, trumpSuit);

            foreach (var meld in possibleMelds)
            {
                melds.Add(new PossibleMeld
                {
                    Type = meld.Type,
                    Points = meld.Points,
                    Cards = meld.Cards.ToList()
                });
            }

            return melds;
        }

        public bool IsLastNineCardsPhase()
        {
            return _deckOps.GetRemainingCardCount() == 0 && _deckOps.GetTrumpCard() == null;
        }

        public string GetRulesText()
        {
            return @"
[bold yellow]📜 BEZIQUE CARD GAME RULES[/]

[bold]Deck:[/] 4 decks + 4 jokers (132 cards total)
[bold]Card Order (high to low):[/] Ace → 10 → King → Queen → Jack → 9 → 8 → 7

[bold]Setup:[/]
• 9 cards dealt to each player (in sets of 3)
• Top card flipped determines trump suit
• Non-dealer plays first card

[bold]Gameplay Flow:[/]
1. [yellow]Play a card[/] - Each player plays one card to form a trick
2. [yellow]Win the trick[/] - Highest trump or highest led suit wins
3. [yellow]Declare meld[/] - Winner can declare a meld for bonus points
4. [yellow]Draw cards[/] - Winner draws first, then loser
5. [yellow]Repeat[/] - Continue until deck is empty, then play last 9 cards

[bold]Melds & Points:[/]
• Trump Run (A,10,K,Q,J)        [green]250 pts[/]
• Double Bezique (2× Q♠ + J♦)   [green]500 pts[/]
• Four Aces                     [green]100 pts[/]
• Four Kings                    [green]80 pts[/]
• Four Queens                   [green]60 pts[/]
• Four Jacks                    [green]40 pts[/]
• Trump Marriage (K+Q)          [green]40 pts[/]
• Bezique (Q♠ + J♦)            [green]40 pts[/]
• Marriage (non-trump)          [green]20 pts[/]
• 7 of Trump (switch)           [green]10 pts[/]

[bold]Special Rules:[/]
• [yellow]Joker:[/] Can be played anytime. Only trump can beat a joker
• [yellow]7 of Trump Switch:[/] Exchange with face-up trump (+10 pts)
• [yellow]Last 9 Cards:[/] Must follow suit with higher card if possible
• [yellow]Last Trick Bonus:[/] +10 points for winning the final trick

[bold]Winning:[/] First player to reach 1500 points wins!

[bold cyan]🤖 AI Opponents:[/]
• [yellow]Easy AI:[/] Plays randomly, good for learning
• [yellow]Medium AI:[/] Plays strategically, fair challenge
• [yellow]Hard AI:[/] Plays optimally, tough opponent
• [yellow]Expert AI:[/] Uses advanced strategy, ultimate challenge
";
        }

        public class PossibleMeld
        {
            public MeldType Type { get; set; }
            public int Points { get; set; }
            public List<Card> Cards { get; set; } = new();
        }
    }
}
