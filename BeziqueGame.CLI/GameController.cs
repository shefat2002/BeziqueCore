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
using BeziqueCore.AI;
using BeziqueCore.Helpers;

namespace BeziqueGame.CLI
{
    /// <summary>
    /// Main game controller with single-player bot support.
    /// Demonstrates SDK consumption for both human and AI players.
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

        // Bot Players (mapping from Player to Bot)
        private readonly Dictionary<Player, IBeziqueBot> _bots;
        private Player _humanPlayer;

        public GameController()
        {
            // Initialize SDK components
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
            _bots = new Dictionary<Player, IBeziqueBot>();
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
                        .AddChoices(new[] { "Single Player (vs AI)", "Multiplayer (Local)", "Show Rules", "Exit" })
                );

                switch (choice)
                {
                    case "Single Player (vs AI)":
                        StartSinglePlayerGame();
                        break;
                    case "Multiplayer (Local)":
                        StartMultiplayerGame();
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

        private void StartSinglePlayerGame()
        {
            AnsiConsole.Clear();
            PrintHeader();

            // Get player name
            var playerName = AnsiConsole.Ask<string>("Enter your name:");
            if (string.IsNullOrWhiteSpace(playerName))
                playerName = "Player 1";

            // Select bot difficulty
            var botDifficulty = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold yellow]Select AI difficulty:[/]")
                    .AddChoices(new[] { "Easy", "Medium", "Hard", "Expert" })
            );

            // Initialize game with 1 human + 1 bot
            InitializeSinglePlayerGame(playerName, botDifficulty);

            // Start game loop
            RunGameLoop();
        }

        private void StartMultiplayerGame()
        {
            AnsiConsole.Clear();
            PrintHeader();

            var playerCount = AnsiConsole.Prompt(
                new SelectionPrompt<int>()
                    .Title("[bold yellow]How many players?[/]")
                    .AddChoices(new[] { 2, 4 })
            );

            InitializeMultiplayerGame(playerCount);
            RunGameLoop();
        }

        private void InitializeSinglePlayerGame(string humanName, string difficulty)
        {
            // Reset and initialize
            _deckOps.InitializeDeck();
            _gameState.Reset();
            _bots.Clear();

            // Create human player
            _humanPlayer = new Player
            {
                Id = Guid.NewGuid().ToString(),
                Name = humanName,
                Score = 0,
                Hand = new List<Card>(),
                DeclaredMelds = new List<Meld>(),
                MeldedCards = new List<Card>(),
                IsDealer = false
            };
            _gameState.AddPlayer(_humanPlayer);

            // Create bot player
            var bot = new Player
            {
                Id = Guid.NewGuid().ToString(),
                Name = $"{difficulty} AI",
                Score = 0,
                Hand = new List<Card>(),
                DeclaredMelds = new List<Meld>(),
                MeldedCards = new List<Card>(),
                IsDealer = true
            };

            // Create bot AI based on difficulty
            IBeziqueBot botAI = difficulty switch
            {
                "Easy" => new EasyBot(),
                "Medium" => new MediumBot(),
                "Hard" => new HardBot(),
                "Expert" => new ExpertBot(),
                _ => new MediumBot()
            };

            _bots[bot] = botAI;
            _gameState.AddPlayer(bot);

            // Set current player (non-dealer human leads first)
            _gameState.CurrentPlayer = _humanPlayer;

            // Start state machine
            _stateMachine.Start();
            _stateMachine.DispatchGameInitialized();
        }

        private void InitializeMultiplayerGame(int playerCount)
        {
            _deckOps.InitializeDeck();
            _gameState.Reset();
            _bots.Clear();

            for (int i = 0; i < playerCount; i++)
            {
                var name = AnsiConsole.Ask<string>($"Player {i + 1} name:");
                if (string.IsNullOrWhiteSpace(name))
                    name = $"Player {i + 1}";

                var player = new Player
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = name,
                    Score = 0,
                    Hand = new List<Card>(),
                    DeclaredMelds = new List<Meld>(),
                    MeldedCards = new List<Card>(),
                    IsDealer = (i == playerCount - 1)
                };

                _gameState.AddPlayer(player);
            }

            var firstPlayer = _gameState.Players.FirstOrDefault(p => !p.IsDealer);
            if (firstPlayer != null)
            {
                _gameState.CurrentPlayer = firstPlayer;
            }

            _stateMachine.Start();
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
                    // Use SDK's built-in trick completion check
                    _stateMachine.CheckAndDispatchTrickComplete();
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
                    // Use SDK's built-in trick completion check for Last 9 Cards phase
                    _stateMachine.CheckAndDispatchTrickComplete();
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
                    break;
            }
        }

        private void HandlePlayerTurn()
        {
            var player = _gameState.CurrentPlayer;
            if (player == null) return;

            // Check if this is a bot player
            if (_bots.ContainsKey(player))
            {
                HandleBotTurn(player);
                return;
            }

            // Human player turn
            AnsiConsole.Clear();
            PrintHeader();
            _ui.DisplayGameInfo();

            AnsiConsole.MarkupLine($"\n[bold blue]🃏 {player.Name}'s Turn[/]\n");

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

        private void HandleBotTurn(Player botPlayer)
        {
            // Get the bot AI
            var bot = _bots[botPlayer];

            // Bot thinks...
            Thread.Sleep(1000); // Simulate thinking time

            // Select card to play
            var cardToPlay = bot.SelectCardToPlay(
                botPlayer,
                _gameState.CurrentTrick,
                _gameState.TrumpSuit,
                _deckOps.GetRemainingCardCount() == 0
            );

            // Play the card
            _playerActions.PlayCard(botPlayer, cardToPlay);

            // Show what bot played
            AnsiConsole.MarkupLine($"\n[bold cyan]{botPlayer.Name}[/] plays [dim]{_ui.GetCardDisplay(cardToPlay)}[/]");
            _ui.DisplayTrickFormation();

            Thread.Sleep(1500);
            _stateMachine.DispatchCardPlayed();
        }

        private void HandleL9PlayerTurn()
        {
            var player = _gameState.CurrentPlayer;
            if (player == null) return;

            // Check if this is a bot player
            if (_bots.ContainsKey(player))
            {
                HandleBotL9Turn(player);
                return;
            }

            // Human player turn
            AnsiConsole.Clear();
            PrintHeader();
            _ui.DisplayGameInfo();

            AnsiConsole.MarkupLine("\n[bold red]⚠️  LAST 9 CARDS PHASE - STRICT PLAY RULES[/]\n");
            AnsiConsole.MarkupLine($"[bold blue]🃏 {player.Name}'s Turn[/]\n");

            _ui.DisplayTrickFormation();
            _ui.DisplayPlayerHand(player);

            // Get valid cards
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

        private void HandleBotL9Turn(Player botPlayer)
        {
            var bot = _bots[botPlayer];
            Thread.Sleep(1000);

            var cardToPlay = bot.SelectCardToPlay(
                botPlayer,
                _gameState.CurrentTrick,
                _gameState.TrumpSuit,
                true // isLastNineCardsPhase
            );

            _playerActions.PlayCard(botPlayer, cardToPlay);
            AnsiConsole.MarkupLine($"\n[bold cyan]{botPlayer.Name}[/] plays [dim]{_ui.GetCardDisplay(cardToPlay)}[/]");
            _ui.DisplayTrickFormation();

            Thread.Sleep(1500);
            _stateMachine.DispatchCardPlayed();
        }

        private void HandleTrickResolution()
        {
            AnsiConsole.Clear();
            PrintHeader();
            AnsiConsole.MarkupLine("\n[bold yellow]🏆 Resolving Trick...[/]\n");
            _ui.DisplayTrickFormation();

            if (_gameState.CurrentTrick.Count == 0)
            {
                AnsiConsole.MarkupLine("\n[dim]No cards in trick. Skipping...[/]");
                AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
                Console.ReadKey(true);
                _stateMachine.DispatchTrickResolved();
                return;
            }

            var leadSuit = _gameState.LeadSuit ?? _gameState.CurrentTrick.Values.First().Suit;

            var winner = _trickResolver.DetermineTrickWinner(
                _gameState.CurrentTrick,
                _gameState.TrumpSuit,
                leadSuit
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

            if (_gameState.CurrentTrick.Count == 0)
            {
                _stateMachine.DispatchTrickResolved();
                return;
            }

            var leadSuit = _gameState.LeadSuit ?? _gameState.CurrentTrick.Values.First().Suit;

            var winner = _trickResolver.DetermineTrickWinner(
                _gameState.CurrentTrick,
                _gameState.TrumpSuit,
                leadSuit
            );

            if (winner != null)
            {
                _gameState.LastTrickWinner = winner;
                var points = _trickResolver.CalculateTrickPoints(_gameState.CurrentTrick.Values.ToList());
                winner.Score += points;
                AnsiConsole.MarkupLine($"\n[bold green]✨ {winner.Name} wins the trick! (+{points} points)[/]");

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
            var player = _gameState.LastTrickWinner ?? _gameState.CurrentPlayer;
            if (player == null)
            {
                _stateMachine.DispatchMeldSkipped();
                return;
            }

            AnsiConsole.MarkupLine($"\n[bold magenta]🎨 {player.Name} - Meld Opportunity[/]\n");

            // Check if this is a bot
            if (_bots.ContainsKey(player))
            {
                HandleBotMeld(player);
                return;
            }

            // Human meld opportunity
            _ui.DisplayPlayerHand(player);

            var availableMelds = _ui.GetAvailableMelds(player);

            if (availableMelds.Count > 0)
            {
                AnsiConsole.MarkupLine("\n[bold green]Available Melds:[/]");
                foreach (var meld in availableMelds)
                {
                    var status = meld.AlreadyMeldedCount > 0
                        ? $"[dim]({meld.AlreadyMeldedCount} already melded, {meld.NewCards.Count} new)[/]"
                        : "[green](all new)[/]";

                    AnsiConsole.MarkupLine($"  [yellow]{meld.Type}[/] - {meld.Points} pts {status}");

                    // Show the cards in the meld
                    var cardList = string.Join(", ", meld.Cards.Select(c => _ui.GetCardDisplay(c)));
                    AnsiConsole.MarkupLine($"    [dim]Cards: {cardList}[/]");
                }

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("\n[bold yellow]What would you like to do?[/]")
                        .AddChoices(new[] { "Declare Best Meld", "Skip Meld" })
                );

                if (choice == "Declare Best Meld")
                {
                    // Get best meld (highest points)
                    var bestMeld = availableMelds.OrderByDescending(m => m.Points).First();
                    var meld = new Meld
                    {
                        Type = bestMeld.Type,
                        Cards = bestMeld.Cards,
                        Points = bestMeld.Points
                    };

                    _playerActions.DeclareMeld(player, meld);

                    var newCardsMsg = bestMeld.AlreadyMeldedCount > 0
                        ? $" ({bestMeld.NewCards.Count} new card{(bestMeld.NewCards.Count > 1 ? "s" : "")})"
                        : "";

                    AnsiConsole.MarkupLine($"\n[bold green]✓ Meld declared: {meld.Type} (+{meld.Points} points){newCardsMsg}[/]");
                    AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
                    Console.ReadKey(true);

                    _stateMachine.DispatchMeldDeclared();
                }
                else
                {
                    _stateMachine.DispatchMeldSkipped();
                }
            }
            else
            {
                // No melds available - auto skip
                AnsiConsole.MarkupLine("\n[dim]No valid melds available. Skipping...[/]");
                Thread.Sleep(1000);
                _stateMachine.DispatchMeldSkipped();
            }
        }

        private void HandleBotMeld(Player botPlayer)
        {
            var bot = _bots[botPlayer];

            Thread.Sleep(800);

            var meld = bot.DecideMeld(botPlayer, _gameState.TrumpSuit);

            if (meld != null)
            {
                _playerActions.DeclareMeld(botPlayer, meld);
                AnsiConsole.MarkupLine($"\n[bold cyan]{botPlayer.Name}[/] declared [yellow]{meld.Type}[/] (+{meld.Points} points)");
                Thread.Sleep(1500);
                _stateMachine.DispatchMeldDeclared();
            }
            else
            {
                // No meld available or bot chose to skip
                AnsiConsole.MarkupLine($"\n[dim]{botPlayer.Name} skips meld (no valid melds)[/]");
                Thread.Sleep(1000);
                _stateMachine.DispatchMeldSkipped();
            }
        }

        private void HandleCardDraw()
        {
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

        private void HandleDeckCheck()
        {
            _stateMachine.CheckAndDispatchDeckEmpty();
        }

        private void HandleL9TrickCheck()
        {
            _stateMachine.CheckAndDispatchL9TrickComplete();
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

        private void MoveToNextPlayer()
        {
            // Find current player index and move to next
            var players = _gameState.Players;
            var currentIndex = players.FindIndex(p => p.Id == _gameState.CurrentPlayer?.Id);
            if (currentIndex >= 0)
            {
                var nextIndex = (currentIndex + 1) % players.Count;
                _gameState.CurrentPlayer = players[nextIndex];
            }
        }

        private void HandleNextPlayerTurn()
        {
            // Move to next player
            MoveToNextPlayer();

            // Get the next player
            var nextPlayer = _gameState.CurrentPlayer;
            if (nextPlayer == null) return;

            // Check if this is a bot player
            if (_bots.ContainsKey(nextPlayer))
            {
                // Bot plays immediately
                HandleBotTurn(nextPlayer);
            }
            else
            {
                // Human player - their turn will be handled in next loop iteration
                // The state machine is still in OPPONENT_RESPONSE, but we've updated CurrentPlayer
                // Next iteration will show the same state but with different player
            }
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
