using Spectre.Console;
using BeziqueCore.Models;
using BeziqueCore.Interfaces;
using BeziqueCore.Adapters;
using BeziqueCore.Actions;
using BeziqueCore.Deck;
using BeziqueCore.Notifiers;
using BeziqueCore.Validators;
using BeziqueCore.Resolvers;
using BeziqueCore.Timers;

namespace BeziqueCore.CLI
{
    /// <summary>
    /// Main game controller that consumes the BeziqueCore SDK.
    /// This demonstrates how a Unity developer would integrate the SDK.
    /// </summary>
    public class GameController
    {
        // SDK Core Components
        private readonly IDeckOperations _deckOps;
        private readonly IGameState _gameState;
        private readonly IPlayerActions _playerActions;
        private readonly IMeldValidator _meldValidator;
        private readonly ITrickResolver _trickResolver;
        private readonly IGameStateNotifier _notifier;
        private readonly IPlayerTimer _playerTimer;

        // State Machine
        private readonly BeziqueGameFlow _stateMachine;

        // UI Components
        private readonly GameUI _ui;

        public GameController()
        {
            // Initialize SDK components (like Unity's Awake/Start)
            _deckOps = new DeckOperations();
            _playerTimer = new PlayerTimer();
            _gameState = new GameState(_playerTimer);
            _meldValidator = new MeldValidator();
            _notifier = new GameNotifier();
            _trickResolver = new TrickResolver(_gameState);
            _playerActions = new PlayerActions(_deckOps, _meldValidator, _notifier, _gameState);

            // Create GameAdapter for state machine
            var gameAdapter = new GameAdapter(_deckOps, _playerActions, _notifier, _gameState, _trickResolver);

            // Create state machine with adapter
            _stateMachine = new BeziqueGameFlow(gameAdapter);

            _ui = new GameUI(_gameState, _deckOps, _meldValidator);
        }

        public void ShowMainMenu()
        {
            while (true)
            {
                AnsiConsole.Clear();
                PrintHeader();

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold green]What would you like to do?[/]")
                        .AddChoices(new[] { "Start New Game", "Show Rules", "Exit" })
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
        }

        private void StartNewGame()
        {
            AnsiConsole.Clear();
            PrintHeader();

            var playerCount = AnsiConsole.Prompt(
                new SelectionPrompt<int>()
                    .Title("[bold yellow]How many players?[/]")
                    .AddChoices(new[] { 2, 4 })
            );

            // Initialize SDK
            InitializeGame(playerCount);

            // Start game loop
            RunGameLoop();
        }

        private void InitializeGame(int playerCount)
        {
            // Reset and initialize deck
            _deckOps.InitializeDeck();
            _gameState.Reset();

            // Create players
            for (int i = 0; i < playerCount; i++)
            {
                var name = AnsiConsole.Ask<string>($"Player {i + 1} name:");
                if (string.IsNullOrWhiteSpace(name))
                    name = $"Player {i + 1}";

                var player = new Player
                {
                    Id = System.Guid.NewGuid().ToString(),
                    Name = name,
                    Score = 0,
                    Hand = new List<Card>(),
                    DeclaredMelds = new List<Meld>(),
                    IsDealer = (i == playerCount - 1)
                };

                _gameState.AddPlayer(player);
            }

            // Set non-dealer as current player (leads first trick)
            var firstPlayer = _gameState.Players.FirstOrDefault(p => !p.IsDealer);
            if (firstPlayer != null)
            {
                _gameState.CurrentPlayer = firstPlayer;
            }

            // Start state machine
            _stateMachine.Start();

            // Deal cards via state machine
            _stateMachine.DispatchGameInitialized();
        }

        private void RunGameLoop()
        {
            while (true)
            {
                ProcessCurrentState();
            }
        }

        private void ProcessCurrentState()
        {
            var currentState = _stateMachine.stateId.ToString();

            switch (currentState)
            {
                case "GAME_INIT":
                    _stateMachine.DispatchGameInitialized();
                    break;

                case "DEALING":
                    _stateMachine.DispatchCardsDealt();
                    break;

                case "TRUMP_SELECTION":
                    _stateMachine.DispatchTrumpDetermined();
                    break;

                case "PLAYER_TURN":
                    HandlePlayerTurn();
                    break;

                case "OPPONENT_RESPONSE":
                    HandleOpponentResponse();
                    break;

                case "TRICK_RESOLUTION":
                    HandleTrickResolution();
                    break;

                case "MELD_OPPORTUNITY":
                    HandleMeldOpportunity();
                    break;

                case "MELD_SCORING":
                    _stateMachine.DispatchMeldScored();
                    break;

                case "CARD_DRAW":
                    HandleCardDraw();
                    break;

                case "DECK_CHECK":
                    _stateMachine.CheckAndDispatchDeckEmpty();
                    break;

                case "L9_PLAYER_TURN":
                    HandleL9PlayerTurn();
                    break;

                case "L9_OPPONENT_RESPONSE":
                    _stateMachine.DispatchAllPlayersResponded();
                    break;

                case "L9_TRICK_RESOLUTION":
                    HandleL9TrickResolution();
                    break;

                case "L9_TRICK_CHECK":
                    _stateMachine.CheckAndDispatchL9TrickComplete();
                    break;

                case "ROUND_END":
                    HandleRoundEnd();
                    break;

                case "GAME_OVER":
                    HandleGameOver();
                    break;

                default:
                    // Auto-transition through intermediate states
                    break;
            }
        }

        private void HandlePlayerTurn()
        {
            AnsiConsole.Clear();
            PrintHeader();
            _ui.DisplayGameInfo();

            var player = _gameState.CurrentPlayer;
            if (player == null) return;

            _ui.DisplayPlayerHand(player);

            // Info menu
            var infoChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold cyan]View Info:[/]")
                    .AddChoices(new[] { "Continue to Play", "View All Hands", "Game Status", "Trump Info" })
            );

            switch (infoChoice)
            {
                case "View All Hands":
                    _ui.DisplayAllHands();
                    AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
                    Console.ReadKey(true);
                    return;
                case "Game Status":
                    _ui.DisplayGameStatus();
                    AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
                    Console.ReadKey(true);
                    return;
                case "Trump Info":
                    _ui.DisplayTrumpInfo();
                    AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
                    Console.ReadKey(true);
                    return;
            }

            // Play card
            AnsiConsole.MarkupLine("\n[bold yellow]Select a card to play:[/]");

            var cardChoices = Enumerable.Range(1, player.Hand.Count).ToList();
            var cardIndex = AnsiConsole.Prompt(
                new SelectionPrompt<int>()
                    .AddChoices(cardChoices)
                    .UseConverter(i => $"{i}. {_ui.GetCardDisplay(player.Hand[i - 1])}")
            );

            var selectedCard = player.Hand[cardIndex - 1];

            // Use SDK to play card
            _playerActions.PlayCard(player, selectedCard);

            _ui.DisplayTrickFormation();
            AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
            Console.ReadKey(true);

            _stateMachine.DispatchCardPlayed();
        }

        private void HandleL9PlayerTurn()
        {
            AnsiConsole.Clear();
            PrintHeader();
            _ui.DisplayGameInfo();

            AnsiConsole.MarkupLine("\n[bold red]⚠️  LAST 9 CARDS PHASE - STRICT PLAY RULES[/]\n");

            var player = _gameState.CurrentPlayer;
            if (player == null) return;

            _ui.DisplayTrickFormation();
            _ui.DisplayPlayerHand(player);

            // Get valid cards based on strict rules
            var validCards = _ui.GetValidCardsForL9(player);
            if (validCards.Count < player.Hand.Count)
            {
                AnsiConsole.MarkupLine("\n[yellow]⚡ Valid plays (strict rules):[/]");
                foreach (var card in validCards)
                {
                    AnsiConsole.MarkupLine($"  [green]✓[/] {_ui.GetCardDisplay(card)}");
                }
            }

            AnsiConsole.MarkupLine("\n[bold yellow]Select a card to play:[/]");

            var cardChoices = Enumerable.Range(1, player.Hand.Count).ToList();
            var cardIndex = AnsiConsole.Prompt(
                new SelectionPrompt<int>()
                    .AddChoices(cardChoices)
                    .UseConverter(i =>
                    {
                        var testCard = player.Hand[i - 1];
                        var isValid = validCards.Contains(testCard);
                        var prefix = isValid ? "[green]" : "[red]";
                        var suffix = isValid ? "" : " [dim](invalid)[/]";
                        return $"{i}. {prefix}{_ui.GetCardDisplay(testCard)}{suffix}";
                    })
            );

            var selectedCard = player.Hand[cardIndex - 1];

            // Validate
            if (!validCards.Contains(selectedCard))
            {
                AnsiConsole.MarkupLine("\n[red]❌ Invalid card! You must follow suit with a higher card if possible.[/]");
                AnsiConsole.MarkupLine("[dim]Press any key to try again...[/]");
                Console.ReadKey(true);
                return;
            }

            _playerActions.PlayCard(player, selectedCard);
            _ui.DisplayTrickFormation();
            AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
            Console.ReadKey(true);

            _stateMachine.DispatchCardPlayed();
        }

        private void HandleOpponentResponse()
        {
            _stateMachine.DispatchAllPlayersResponded();
        }

        private void HandleTrickResolution()
        {
            AnsiConsole.Clear();
            PrintHeader();
            AnsiConsole.MarkupLine("\n[bold yellow]🏆 Resolving Trick...[/]\n");
            _ui.DisplayTrickFormation();

            // SDK resolves trick
            var winner = _trickResolver.DetermineTrickWinner(
                _gameState.CurrentTrick,
                _gameState.TrumpSuit,
                _gameState.LeadSuit ?? _gameState.CurrentTrick.Values.First().Suit
            );

            if (winner != null)
            {
                _gameState.LastTrickWinner = winner;
                var points = _trickResolver.CalculateTrickPoints(_gameState.CurrentTrick.Values.ToList());
                winner.Score += points;
                AnsiConsole.MarkupLine($"\n[bold green]✨ {winner.Name} wins the trick! (+{points} points)[/]");
            }

            AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
            Console.ReadKey(true);

            _stateMachine.DispatchTrickResolved();
        }

        private void HandleL9TrickResolution()
        {
            AnsiConsole.Clear();
            PrintHeader();
            AnsiConsole.MarkupLine("\n[bold red]⚠️  LAST 9 CARDS - Resolving Trick...[/]\n");
            _ui.DisplayTrickFormation();

            // SDK resolves trick
            var winner = _trickResolver.DetermineTrickWinner(
                _gameState.CurrentTrick,
                _gameState.TrumpSuit,
                _gameState.LeadSuit ?? _gameState.CurrentTrick.Values.First().Suit
            );

            if (winner != null)
            {
                _gameState.LastTrickWinner = winner;
                var points = _trickResolver.CalculateTrickPoints(_gameState.CurrentTrick.Values.ToList());
                winner.Score += points;
                AnsiConsole.MarkupLine($"\n[bold green]✨ {winner.Name} wins the trick! (+{points} points)[/]");

                // Check if hands are empty for last trick bonus
                var allHandsEmpty = _gameState.Players.All(p => p.Hand.Count == 0);
                if (allHandsEmpty)
                {
                    winner.Score += 10;
                    AnsiConsole.MarkupLine("[bold yellow]🎯 Last trick bonus: +10 points![/]");
                }
            }

            AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
            Console.ReadKey(true);

            _stateMachine.DispatchTrickResolved();
        }

        private void HandleMeldOpportunity()
        {
            AnsiConsole.Clear();
            PrintHeader();
            _ui.DisplayGameInfo();

            var player = _gameState.LastTrickWinner ?? _gameState.CurrentPlayer;
            if (player == null)
            {
                _stateMachine.DispatchMeldSkipped();
                return;
            }

            AnsiConsole.MarkupLine($"\n[bold magenta]🎨 {player.Name} - Meld Opportunity[/]\n");
            _ui.DisplayPlayerHand(player);

            // Check available melds using SDK
            var availableMelds = _ui.GetAvailableMelds(player);

            if (availableMelds.Count > 0)
            {
                AnsiConsole.MarkupLine("\n[bold green]Available Melds:[/]");
                foreach (var meld in availableMelds)
                {
                    AnsiConsole.MarkupLine($"  [yellow]{meld.Type}[/] - {meld.Points} pts");
                }
            }
            else
            {
                AnsiConsole.MarkupLine("\n[dim]No valid melds available.[/]");
            }

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("\n[bold yellow]What would you like to do?[/]")
                    .AddChoices(new[] { "Declare Meld", "Skip Meld" })
            );

            if (choice == "Declare Meld")
            {
                if (availableMelds.Count == 0)
                {
                    AnsiConsole.MarkupLine("\n[red]No melds available! Skipping...[/]");
                    Thread.Sleep(1000);
                    _stateMachine.DispatchMeldSkipped();
                    return;
                }

                // Declare best meld
                var bestMeld = availableMelds.OrderByDescending(m => m.Points).First();

                // Convert PossibleMeld to actual Meld
                var meld = new Meld
                {
                    Type = bestMeld.Type,
                    Cards = bestMeld.Cards,
                    Points = bestMeld.Points
                };

                _playerActions.DeclareMeld(player, meld);

                AnsiConsole.MarkupLine($"\n[bold green]✓ Meld declared: {bestMeld.Type} (+{bestMeld.Points} points)[/]");
                AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
                Console.ReadKey(true);

                _stateMachine.DispatchMeldDeclared();
            }
            else
            {
                _stateMachine.DispatchMeldSkipped();
            }
        }

        private void HandleCardDraw()
        {
            // Winner draws first, then loser
            var winner = _gameState.LastTrickWinner;
            if (winner == null)
            {
                _stateMachine.DispatchCardsDrawn();
                return;
            }

            var loser = _gameState.Players.FirstOrDefault(p => p != winner);
            if (loser == null)
            {
                _stateMachine.DispatchCardsDrawn();
                return;
            }

            // Winner draws
            if (_deckOps.GetRemainingCardCount() > 0)
            {
                var card = _deckOps.DrawTopCard();
                if (card != null)
                {
                    winner.Hand.Add(card);
                    AnsiConsole.MarkupLine($"[dim]{winner.Name} drew {_ui.GetCardDisplay(card)}[/]");
                }
            }
            else if (_deckOps.GetTrumpCard() != null)
            {
                var trumpCard = _deckOps.TakeTrumpCard();
                if (trumpCard != null)
                {
                    winner.Hand.Add(trumpCard);
                    AnsiConsole.MarkupLine($"[dim]{winner.Name} took trump card {_ui.GetCardDisplay(trumpCard)}[/]");
                }
            }

            // Loser draws
            if (_deckOps.GetRemainingCardCount() > 0)
            {
                var card = _deckOps.DrawTopCard();
                if (card != null)
                {
                    loser.Hand.Add(card);
                    AnsiConsole.MarkupLine($"[dim]{loser.Name} drew {_ui.GetCardDisplay(card)}[/]");
                }
            }
            else if (_deckOps.GetTrumpCard() != null)
            {
                var trumpCard = _deckOps.TakeTrumpCard();
                if (trumpCard != null)
                {
                    loser.Hand.Add(trumpCard);
                    AnsiConsole.MarkupLine($"[dim]{loser.Name} took trump card {_ui.GetCardDisplay(trumpCard)}[/]");
                }
            }

            AnsiConsole.MarkupLine($"\n[dim]Cards remaining in deck: {_deckOps.GetRemainingCardCount()}[/]");
            AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
            Console.ReadKey(true);

            _stateMachine.DispatchCardsDrawn();
        }

        private void HandleRoundEnd()
        {
            AnsiConsole.Clear();
            PrintHeader();
            AnsiConsole.MarkupLine("\n[bold yellow]📊 ROUND ENDED[/]\n");

            _ui.DisplayScores();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("\n[bold green]Continue to next round or end game?[/]")
                    .AddChoices(new[] { "Continue Game", "End Game" })
            );

            if (choice == "Continue Game")
            {
                _stateMachine.DispatchContinueGame();
                ResetForNewRound();
            }
            else
            {
                _stateMachine.DispatchWinningScoreReached();
            }
        }

        private void HandleGameOver()
        {
            AnsiConsole.Clear();
            PrintHeader();
            AnsiConsole.MarkupLine("\n[bold green]🎉 GAME OVER![/]\n");

            _ui.DisplayScores();

            var winner = _gameState.Players.OrderByDescending(p => p.Score).First();
            AnsiConsole.MarkupLine($"\n[bold yellow]🏆 {winner.Name} wins with {winner.Score} points![/]");

            AnsiConsole.MarkupLine("\n[dim]Press any key to return to main menu...[/]");
            Console.ReadKey(true);
        }

        private void ResetForNewRound()
        {
            foreach (var player in _gameState.Players)
            {
                player.Hand.Clear();
                player.DeclaredMelds?.Clear();
                player.IsDealer = !player.IsDealer;
            }

            var newCurrent = _gameState.Players.FirstOrDefault(p => !p.IsDealer);
            if (newCurrent != null)
            {
                _gameState.CurrentPlayer = newCurrent;
            }

            _gameState.StartNewTrick();
        }

        private void ShowRules()
        {
            AnsiConsole.Clear();
            var rules = new Panel(_ui.GetRulesText());
            rules.Border = BoxBorder.Rounded;
            rules.Header = new PanelHeader("[bold yellow]📖 GAME RULES[/]");
            AnsiConsole.Write(rules);

            AnsiConsole.MarkupLine("\n[dim]Press any key to return...[/]");
            Console.ReadKey(true);
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
