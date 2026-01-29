using Spectre.Console;
using System.Text;
using BeziqueCore.Models;
using BeziqueCore.Interfaces;

namespace BeziqueCore.CLI
{
    public class GameMenu
    {
        private readonly GameConsole _gameConsole;
        private readonly BeziqueGameFlow _stateMachine;

        public GameMenu(GameConsole gameConsole, BeziqueGameFlow stateMachine)
        {
            _gameConsole = gameConsole;
            _stateMachine = stateMachine;
        }

        public void ShowMainMenu()
        {
            AnsiConsole.Clear();
            PrintHeader();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold green]What would you like to do?[/]")
                    .AddChoices(new[] {
                        "Start New Game",
                        "Show Rules",
                        "Exit"
                    })
                    .UseConverter<string>(choice => choice)
            );

            switch (choice)
            {
                case "Start New Game":
                    StartNewGame();
                    break;
                case "Show Rules":
                    ShowRules();
                    break;
                case "Exit":
                    Environment.Exit(0);
                    break;
            }
        }

        private void StartNewGame()
        {
            AnsiConsole.MarkupLine("[dim]Entering StartNewGame...[/]");
            AnsiConsole.Clear();
            PrintHeader();

            var playerCount = AnsiConsole.Prompt(
                new SelectionPrompt<int>()
                    .Title("[bold yellow]How many players?[/]")
                    .AddChoices(new[] { 2, 4 })
            );

            AnsiConsole.MarkupLine($"[dim]Selected {playerCount} players. Setting up...[/]");
            SetupPlayers(playerCount);
            RunGameLoop();
        }

        private void ShowRules()
        {
            AnsiConsole.Clear();
            var rules = new Panel(@"
[bold yellow]📜 BEZIQUE CARD GAME RULES[/]

[bold]Deck:[/] 4 decks + 4 jokers (132 cards total)
[bold]Card Order (high to low):[/] Ace → 10 → King → Queen → Jack → 9 → 8 → 7

[bold]Setup:[/]
• 9 cards dealt to each player (in sets of 3)
• Top card flipped determines trump suit

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
• 7 of Trump                    [green]10 pts[/]

[bold]Special Rules:[/]
• [yellow]Joker:[/] Only trump can beat a joker
• [yellow]7 of Trump Switch:[/] Exchange with face-up trump (+10 pts)
• [yellow]Last 9 Cards:[/] Strict play rules apply
• [yellow]Timer:[/] 15 seconds per turn (-10 pts for timeout)

[bold]Winning:[/] First player to reach the agreed score wins!
");

            rules.Border = BoxBorder.Rounded;
            rules.Header = new PanelHeader("[bold yellow]📖 GAME RULES[/]");
            AnsiConsole.Write(rules);

            AnsiConsole.MarkupLine("\n[dim]Press any key to return...[/]");
            Console.ReadKey(true);
            ShowMainMenu();
        }

        private void SetupPlayers(int playerCount)
        {
            AnsiConsole.MarkupLine($"\n[bold cyan]Setting up {playerCount} players...[/]\n");

            for (int i = 0; i < playerCount; i++)
            {
                var name = AnsiConsole.Ask<string>($"Player {i + 1} name:");
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = $"Player {i + 1}";
                }

                var player = new Player
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = name,
                    Score = 0,
                    Hand = new List<Card>(),
                    IsDealer = (i == playerCount - 1)
                };

                _gameConsole.AddPlayer(player);
                AnsiConsole.MarkupLine($"[dim]Added player: {name}[/]");
            }

            _gameConsole.SetCurrentPlayer(0);
            AnsiConsole.MarkupLine($"[dim]Total players in game: {_gameConsole.GetPlayers().Count}[/]");
        }

        public void RunGameLoop()
        {
            AnsiConsole.MarkupLine("[dim]About to start state machine...[/]");
            _stateMachine.Start();
            AnsiConsole.MarkupLine($"[dim]State machine started. Current state: {_stateMachine.stateId}[/]");

            // Initialize the game - dispatch events to progress through initial states
            _gameConsole.InitializeGame();
            _stateMachine.DispatchGameInitialized();
            _gameConsole.DealCards();
            _stateMachine.DispatchCardsDealt();
            _gameConsole.FlipTrumpCard();
            _stateMachine.DispatchTrumpDetermined();

            AnsiConsole.MarkupLine($"[dim]Game initialized. Current state: {_stateMachine.stateId}[/]");

            while (true)
            {
                AnsiConsole.Clear();
                PrintHeader();
                _gameConsole.DisplayGameState();

                ShowGameMenu();
            }
        }

        private void ShowGameMenu()
        {
            var currentState = _stateMachine.stateId.ToString();
            AnsiConsole.MarkupLine($"[dim]Current State: {currentState}[/]\n");

            var options = new List<string> { "View Hands", "Game Status" };

            if (currentState.Contains("PLAYER_TURN") || currentState.Contains("OPPONENT_RESPONSE"))
            {
                options.Add("Play Card");
                options.Add("Declare Meld");
                options.Add("Skip Meld");
            }

            if (currentState.Contains("DECK_CHECK"))
            {
                if (_gameConsole.IsLastNineCardsPhase())
                {
                    options.Add("Last 9 Cards Reached");
                }
                else
                {
                    options.Add("Continue Playing");
                }
            }

            if (currentState.Contains("ROUND_END"))
            {
                options.Add("Continue Game");
                options.Add("Game Over");
            }

            options.Add("Return to Main Menu");

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold green]Choose an action:[/]")
                    .AddChoices(options.ToArray())
            );

            HandleMenuChoice(choice);
        }

        private void HandleMenuChoice(string choice)
        {
            switch (choice)
            {
                case "View Hands":
                    _gameConsole.DisplayHands();
                    AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
                    Console.ReadKey(true);
                    break;

                case "Game Status":
                    ShowDetailedStatus();
                    AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
                    Console.ReadKey(true);
                    break;

                case "Play Card":
                    PlayCard();
                    break;

                case "Declare Meld":
                    DeclareMeld();
                    break;

                case "Skip Meld":
                    _stateMachine.DispatchMeldSkipped();
                    break;

                case "Continue Playing":
                    _stateMachine.DispatchMoreCardsAvailable();
                    break;

                case "Check Deck":
                    _stateMachine.CheckAndDispatchDeckEmpty();
                    break;

                case "Continue Game":
                    _stateMachine.DispatchContinueGame();
                    break;

                case "Game Over":
                    _stateMachine.DispatchWinningScoreReached();
                    break;

                case "Return to Main Menu":
                    ShowMainMenu();
                    break;
            }
        }

        private void PlayCard()
        {
            var player = _gameConsole.GetCurrentPlayer();
            if (player == null) return;

            _gameConsole.DisplayHands();

            var cardIndex = AnsiConsole.Prompt(
                new TextPrompt<int>($"[bold blue]{player.Name}[/], enter card number to play (1-{player.Hand.Count}):")
                    .PromptStyle("blue")
                    .Validate(card => card >= 1 && card <= player.Hand.Count, "[red]Invalid card number[/]")
            );

            var card = player.Hand[cardIndex - 1];
            _gameConsole.PlayerActions.PlayCard(player, card);
            _stateMachine.DispatchCardPlayed();
        }

        private void DeclareMeld()
        {
            var player = _gameConsole.GetCurrentPlayer();
            if (player == null) return;

            AnsiConsole.MarkupLine("[bold magenta]Declare Meld[/]\n");

            var meldTypes = new[] { "Trump Run", "Trump Marriage", "Marriage", "Four Aces", "Four Kings", "Four Queens", "Four Jacks", "Bezique", "Double Bezique", "7 of Trump" };

            var meldTypeStr = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select meld type:")
                    .AddChoices(meldTypes)
            );

            var meldType = meldTypeStr switch
            {
                "Trump Run" => MeldType.TrumpRun,
                "Trump Marriage" => MeldType.TrumpMarriage,
                "Marriage" => MeldType.Marriage,
                "Four Aces" => MeldType.FourAces,
                "Four Kings" => MeldType.FourKings,
                "Four Queens" => MeldType.FourQueens,
                "Four Jacks" => MeldType.FourJacks,
                "Bezique" => MeldType.Bezique,
                "Double Bezique" => MeldType.DoubleBezique,
                "7 of Trump" => MeldType.TrumpSeven,
                _ => MeldType.InvalidMeld
            };

            AnsiConsole.MarkupLine($"[dim]Enter card positions (comma-separated) for {meldTypeStr}:[/]");
            var positions = AnsiConsole.Ask<string>("Cards:");

            var cardIndices = positions.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.Parse(s.Trim()) - 1)
                .ToList();

            var cards = cardIndices.Select(i => player.Hand[i]).ToList();
            var meld = new Meld
            {
                Type = meldType,
                Cards = cards,
                Points = 0
            };

            _gameConsole.PlayerActions.DeclareMeld(player, meld);
            _stateMachine.DispatchMeldDeclared();
            _stateMachine.DispatchMeldScored();
        }

        private void ShowDetailedStatus()
        {
            var sb = new StringBuilder();
            sb.AppendLine("[bold yellow]📊 DETAILED GAME STATUS[/]\n");

            foreach (var player in _gameConsole.GetPlayers())
            {
                sb.AppendLine($"[bold]{player.Name}[/] (Dealer: {(player.IsDealer ? "Yes" : "No")})");
                sb.AppendLine($"  Score: [green]{player.Score}[/]");
                sb.AppendLine($"  Hand: {string.Join(", ", player.Hand.Select(c => GetCardDisplay(c)))}");
                sb.AppendLine($"  Cards in hand: {player.Hand.Count}");
                sb.AppendLine();
            }

            sb.AppendLine($"[bold]Trump:[/] {_gameConsole.GameState.TrumpSuit}");
            sb.AppendLine($"[bold]Deck:[/] {_gameConsole.DeckOps.GetRemainingCardCount()} cards");
            sb.AppendLine($"[bold]Last 9 Phase:[/] {(_gameConsole.IsLastNineCardsPhase() ? "[red]YES[/]" : "[green]NO[/]")}");

            var panel = new Panel(sb.ToString().Trim())
            {
                Border = BoxBorder.Rounded
            };
            panel.Padding = new Padding(1, 1, 1, 1);
            AnsiConsole.Write(panel);
        }

        private string GetCardDisplay(Card card)
        {
            if (card.IsJoker) return "🃏";

            var suit = card.Suit switch
            {
                Suit.Hearts => "♥",
                Suit.Diamonds => "♦",
                Suit.Clubs => "♣",
                Suit.Spades => "♠",
                _ => "?"
            };

            return $"{suit}{card.Rank}";
        }

        private void PrintHeader()
        {
            var header = new FigletText("BEZIQUE")
            {
                Justification = Justify.Center
            };
            AnsiConsole.Write(header);

            var rule = new Rule("[yellow]Card Game SDK[/]");
            AnsiConsole.Write(rule);
        }
    }
}
