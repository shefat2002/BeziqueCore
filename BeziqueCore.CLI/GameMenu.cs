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
            AnsiConsole.Clear();
            PrintHeader();

            var playerCount = AnsiConsole.Prompt(
                new SelectionPrompt<int>()
                    .Title("[bold yellow]How many players?[/]")
                    .AddChoices(new[] { 2, 4 })
            );

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
• Non-dealer plays first card

[bold]Gameplay Flow:[/]
1. [yellow]Play a card[/] - Each player plays one card to form a trick
2. [yellow]Win the trick[/] - Highest trump or highest led suit wins (Joker beats all except trump)
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
        }

        public void RunGameLoop()
        {
            _stateMachine.Start();

            // Initialize the game
            _gameConsole.InitializeGame();
            _stateMachine.DispatchGameInitialized();

            _gameConsole.DealCards();
            _stateMachine.DispatchCardsDealt();

            _gameConsole.FlipTrumpCard();
            _stateMachine.DispatchTrumpDetermined();

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
                case "GAMEPLAY_PLAYER_TURN":
                    HandlePlayerTurn();
                    break;

                case "GAMEPLAY_OPPONENT_RESPONSE":
                    HandleOpponentResponse();
                    break;

                case "GAMEPLAY_TRICK_RESOLUTION":
                    HandleTrickResolution();
                    break;

                case "GAMEPLAY_MELD_OPPORTUNITY":
                    HandleMeldOpportunity();
                    break;

                case "GAMEPLAY_MELD_SCORING":
                    _stateMachine.DispatchMeldScored();
                    break;

                case "GAMEPLAY_CARD_DRAW":
                    HandleCardDraw();
                    break;

                case "GAMEPLAY_DECK_CHECK":
                    HandleDeckCheck();
                    break;

                case "LAST_9_CARDS_L9_PLAYER_TURN":
                    HandleL9PlayerTurn();
                    break;

                case "LAST_9_CARDS_L9_OPPONENT_RESPONSE":
                    HandleL9OpponentResponse();
                    break;

                case "LAST_9_CARDS_L9_TRICK_RESOLUTION":
                    HandleL9TrickResolution();
                    break;

                case "LAST_9_CARDS_L9_TRICK_CHECK":
                    HandleL9TrickCheck();
                    break;

                case "ROUND_END":
                    HandleRoundEnd();
                    break;

                case "GAME_OVER":
                    HandleGameOver();
                    break;

                default:
                    // Auto-transition through states that don't need user input
                    break;
            }
        }

        private void HandlePlayerTurn()
        {
            AnsiConsole.Clear();
            PrintHeader();
            DisplayGameInfo();

            var player = _gameConsole.GetCurrentPlayer();
            if (player == null) return;

            AnsiConsole.MarkupLine($"\n[bold blue]🃏 {player.Name}'s Turn[/]\n");
            AnsiConsole.MarkupLine("[dim]" + new string('─', Console.WindowWidth - 4) + "[/]\n");

            // Show current player's hand
            DisplayPlayerHand(player);

            // Show info menu
            var infoChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold cyan]View Info:[/]")
                    .AddChoices(new[] { "Continue to Play", "View All Hands", "Game Status", "Trump Info" })
                    .UseConverter(c => c)
            );

            switch (infoChoice)
            {
                case "View All Hands":
                    DisplayAllHands();
                    AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
                    Console.ReadKey(true);
                    HandlePlayerTurn(); // Recursively call to show hand again
                    return;
                case "Game Status":
                    DisplayGameStatus();
                    AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
                    Console.ReadKey(true);
                    HandlePlayerTurn();
                    return;
                case "Trump Info":
                    DisplayTrumpInfo();
                    AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
                    Console.ReadKey(true);
                    HandlePlayerTurn();
                    return;
            }

            // Play a card
            AnsiConsole.MarkupLine("\n[bold yellow]Select a card to play:[/]");

            var cardChoices = Enumerable.Range(1, player.Hand.Count).ToList();
            var cardIndex = AnsiConsole.Prompt(
                new SelectionPrompt<int>()
                    .AddChoices(cardChoices)
                    .UseConverter(i => $"{i}. {GetCardDisplay(player.Hand[i - 1])}")
            );

            var selectedCard = player.Hand[cardIndex - 1];

            // Confirm card play
            AnsiConsole.MarkupLine($"\n[dim]Playing: {GetCardDisplay(selectedCard)}[/]");

            _gameConsole.PlayerActions.PlayCard(player, selectedCard);

            // Show the card played
            DisplayTrickFormation();

            AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
            Console.ReadKey(true);

            _stateMachine.DispatchCardPlayed();
        }

        private void HandleL9PlayerTurn()
        {
            AnsiConsole.Clear();
            PrintHeader();
            DisplayGameInfo();

            AnsiConsole.MarkupLine("\n[bold red]⚠️  LAST 9 CARDS PHASE - STRICT PLAY RULES[/]\n");

            var player = _gameConsole.GetCurrentPlayer();
            if (player == null) return;

            AnsiConsole.MarkupLine($"[bold blue]🃏 {player.Name}'s Turn[/]\n");
            AnsiConsole.MarkupLine("[dim]" + new string('─', Console.WindowWidth - 4) + "[/]\n");

            // Show current trick
            DisplayTrickFormation();

            // Show player's hand
            DisplayPlayerHand(player);

            // Show valid moves based on strict rules
            var validCards = GetValidCardsForL9(player);
            if (validCards.Count < player.Hand.Count)
            {
                AnsiConsole.MarkupLine("\n[yellow]⚡ Valid plays (strict rules):[/]");
                foreach (var card in validCards)
                {
                    AnsiConsole.MarkupLine($"  [green]✓[/] {GetCardDisplay(card)}");
                }
            }

            // Play a card
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
                        return $"{i}. {prefix}{GetCardDisplay(testCard)}{suffix}";
                    })
            );

            var selectedCard = player.Hand[cardIndex - 1];

            // Validate the card
            if (!validCards.Contains(selectedCard))
            {
                AnsiConsole.MarkupLine("\n[red]❌ Invalid card! You must follow suit with a higher card if possible.[/]");
                AnsiConsole.MarkupLine("[dim]Press any key to try again...[/]");
                Console.ReadKey(true);
                HandleL9PlayerTurn(); // Retry
                return;
            }

            _gameConsole.PlayerActions.PlayCard(player, selectedCard);
            DisplayTrickFormation();

            AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
            Console.ReadKey(true);

            _stateMachine.DispatchCardPlayed();
        }

        private void HandleOpponentResponse()
        {
            // For single player testing, auto-respond
            _stateMachine.DispatchAllPlayersResponded();
        }

        private void HandleL9OpponentResponse()
        {
            _stateMachine.DispatchAllPlayersResponded();
        }

        private void HandleTrickResolution()
        {
            AnsiConsole.Clear();
            PrintHeader();

            AnsiConsole.MarkupLine("\n[bold yellow]🏆 Resolving Trick...[/]\n");
            DisplayTrickFormation();

            _gameConsole.ResolveTrick();

            // Show winner
            var winner = _gameConsole.GameState.LastTrickWinner;
            if (winner != null)
            {
                AnsiConsole.MarkupLine($"\n[bold green]✨ {winner.Name} wins the trick![/]");
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
            DisplayTrickFormation();

            _gameConsole.ResolveL9Trick();

            var winner = _gameConsole.GameState.LastTrickWinner;
            if (winner != null)
            {
                AnsiConsole.MarkupLine($"\n[bold green]✨ {winner.Name} wins the trick![/]");

                // Check if hands are empty
                var allHandsEmpty = _gameConsole.AreAllHandsEmpty();
                if (allHandsEmpty)
                {
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
            DisplayGameInfo();

            var player = _gameConsole.GameState.LastTrickWinner ?? _gameConsole.GetCurrentPlayer();
            if (player == null)
            {
                _stateMachine.DispatchMeldSkipped();
                return;
            }

            AnsiConsole.MarkupLine($"\n[bold magenta]🎨 {player.Name} - Meld Opportunity[/]\n");
            AnsiConsole.MarkupLine("[dim]" + new string('─', Console.WindowWidth - 4) + "[/]\n");

            DisplayPlayerHand(player);

            // Check for available melds
            var availableMelds = GetAvailableMelds(player);

            if (availableMelds.Count > 0)
            {
                AnsiConsole.MarkupLine("\n[bold green]Available Melds:[/]");
                foreach (var meld in availableMelds)
                {
                    AnsiConsole.MarkupLine($"  [yellow]{meld.Type}[/] - {meld.Points} pts - Cards: {string.Join(", ", meld.Cards.Select(c => GetCardDisplay(c)))}");
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

                // Select meld to declare (highest point meld first)
                var bestMeld = availableMelds.OrderByDescending(m => m.Points).First();

                var meld = new Meld
                {
                    Type = bestMeld.Type,
                    Cards = bestMeld.Cards,
                    Points = bestMeld.Points
                };

                _gameConsole.PlayerActions.DeclareMeld(player, meld);

                AnsiConsole.MarkupLine($"\n[bold green]✓ Meld declared: {meld.Type} (+{meld.Points} points)[/]");
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
            _gameConsole.DrawCards();

            // Show what was drawn
            var players = _gameConsole.GetPlayers();
            var remainingDeck = _gameConsole.DeckOps.GetRemainingCardCount();

            AnsiConsole.Clear();
            PrintHeader();
            AnsiConsole.MarkupLine("\n[bold cyan]📥 Drawing Cards[/]\n");
            AnsiConsole.MarkupLine($"[dim]Cards remaining in deck: {remainingDeck}[/]");
            AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
            Console.ReadKey(true);

            _stateMachine.DispatchCardsDrawn();
        }

        private void HandleDeckCheck()
        {
            _gameConsole.CheckDeck();
            _stateMachine.CheckAndDispatchDeckEmpty();
        }

        private void HandleL9TrickCheck()
        {
            _gameConsole.CheckL9TrickComplete();
            _stateMachine.CheckAndDispatchL9TrickComplete();
        }

        private void HandleRoundEnd()
        {
            AnsiConsole.Clear();
            PrintHeader();

            AnsiConsole.MarkupLine("\n[bold yellow]📊 ROUND ENDED[/]\n");

            _gameConsole.CalculateRoundScores();

            // Show scores
            var table = new Table();
            table.Border(TableBorder.Rounded);
            table.AddColumn("[bold]Player[/]");
            table.AddColumn("[bold]Round Score[/]");
            table.AddColumn("[bold]Total Score[/]");

            foreach (var player in _gameConsole.GetPlayers())
            {
                table.AddRow(
                    player.Name,
                    player.Score.ToString(),
                    player.Score.ToString()
                );
            }

            AnsiConsole.Write(table);

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

            _gameConsole.DeclareWinner();

            var winner = _gameConsole.GetPlayers().OrderByDescending(p => p.Score).First();
            AnsiConsole.MarkupLine($"[bold yellow]🏆 {winner.Name} wins with {winner.Score} points![/]\n");

            // Final scores
            var table = new Table();
            table.Border(TableBorder.Rounded);
            table.AddColumn("[bold]Player[/]");
            table.AddColumn("[bold]Final Score[/]");

            foreach (var player in _gameConsole.GetPlayers().OrderByDescending(p => p.Score))
            {
                table.AddRow(player.Name, player.Score.ToString());
            }

            AnsiConsole.Write(table);

            AnsiConsole.MarkupLine("\n[dim]Press any key to return to main menu...[/]");
            Console.ReadKey(true);
            ShowMainMenu();
        }

        private List<Card> GetValidCardsForL9(Player player)
        {
            var currentTrick = _gameConsole.GameState.CurrentTrick;

            // No cards played yet - any card is valid
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
            {
                return sameSuitHigher;
            }

            // No higher same suit - can play any same suit card
            var sameSuit = player.Hand.Where(c => c.Suit == leadSuit).ToList();
            if (sameSuit.Any())
            {
                return sameSuit;
            }

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

        private List<PossibleMeld> GetAvailableMelds(Player player)
        {
            var melds = new List<PossibleMeld>();
            var hand = player.Hand;
            var trumpSuit = _gameConsole.GameState.TrumpSuit;

            // Check each meld type
            var meldTypes = new[]
            {
                MeldType.DoubleBezique,
                MeldType.TrumpRun,
                MeldType.FourAces,
                MeldType.FourKings,
                MeldType.FourQueens,
                MeldType.FourJacks,
                MeldType.Bezique,
                MeldType.TrumpMarriage,
                MeldType.Marriage,
                MeldType.TrumpSeven
            };

            foreach (var meldType in meldTypes)
            {
                var meld = _gameConsole.MeldValidator.GetBestPossibleMeld(
                    hand.ToArray(),
                    hand,
                    trumpSuit
                );

                if (meld != null && meld.Type == meldType)
                {
                    melds.Add(new PossibleMeld
                    {
                        Type = meld.Type,
                        Points = meld.Points,
                        Cards = meld.Cards.ToList()
                    });
                }
            }

            return melds;
        }

        private void DisplayGameInfo()
        {
            var info = new Table().Border(TableBorder.None).Width(Console.WindowWidth - 4);
            info.AddColumn(new TableColumn("[bold]Trump:[]").Width(20));
            info.AddColumn(new TableColumn("[bold]Deck:[]").Width(20));
            info.AddColumn(new TableColumn("[bold]Phase:[]").Width(30));

            var trumpDisplay = GetSuitDisplay(_gameConsole.GameState.TrumpSuit);
            var deckCount = _gameConsole.DeckOps.GetRemainingCardCount();
            var phase = _gameConsole.IsLastNineCardsPhase() ? "[red]Last 9 Cards[/]" : "[green]Normal[/]";

            info.AddRow(trumpDisplay, $"{deckCount} cards", phase);
            AnsiConsole.Write(info);
        }

        private void DisplayPlayerHand(Player player)
        {
            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn("[bold]" + player.Name + "'s Hand[/]");

            for (int i = 0; i < player.Hand.Count; i++)
            {
                table.AddRow($"[dim]{i + 1}.[/] {GetCardDisplay(player.Hand[i])}");
            }

            AnsiConsole.Write(table);
        }

        private void DisplayAllHands()
        {
            foreach (var player in _gameConsole.GetPlayers())
            {
                AnsiConsole.MarkupLine($"\n[bold]{player.Name}'s Hand:[/]");
                for (int i = 0; i < player.Hand.Count; i++)
                {
                    AnsiConsole.MarkupLine($"  {i + 1}. {GetCardDisplay(player.Hand[i])}");
                }
            }
        }

        private void DisplayGameStatus()
        {
            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn("[bold]Player[/]");
            table.AddColumn("[bold]Score[/]");
            table.AddColumn("[bold]Cards[/]");
            table.AddColumn("[bold]Dealer[/]");

            foreach (var player in _gameConsole.GetPlayers())
            {
                table.AddRow(
                    player.Name,
                    player.Score.ToString(),
                    player.Hand.Count.ToString(),
                    player.IsDealer ? "[yellow]Yes[/]" : "[dim]No[/]"
                );
            }

            AnsiConsole.Write(table);

            AnsiConsole.MarkupLine($"\n[bold]Trump:[/] {GetSuitDisplay(_gameConsole.GameState.TrumpSuit)}");
            AnsiConsole.MarkupLine($"[bold]Deck:[/] {_gameConsole.DeckOps.GetRemainingCardCount()} cards");

            if (_gameConsole.GameState.CurrentTrick.Count > 0)
            {
                AnsiConsole.MarkupLine("\n[bold]Current Trick:[/]");
                foreach (var trickCard in _gameConsole.GameState.CurrentTrick)
                {
                    AnsiConsole.MarkupLine($"  {trickCard.Key.Name}: {GetCardDisplay(trickCard.Value)}");
                }
            }
        }

        private void DisplayTrumpInfo()
        {
            var trump = _gameConsole.GameState.TrumpSuit;
            var flipped = _gameConsole.GameState.TrumpCard;

            AnsiConsole.MarkupLine($"[bold yellow]Trump Suit:[/] {GetSuitDisplay(trump)}");
            if (flipped != null)
            {
                AnsiConsole.MarkupLine($"[bold]Flipped Card:[/] {GetCardDisplay(flipped)}");
            }
            AnsiConsole.MarkupLine("\n[dim]Cards in this suit are trumps and beat all other suits.[/]");
        }

        private void DisplayTrickFormation()
        {
            if (_gameConsole.GameState.CurrentTrick.Count == 0)
            {
                AnsiConsole.MarkupLine("\n[dim]No cards played yet this trick.[/]");
                return;
            }

            var table = new Table().Border(TableBorder.Rounded).Centered();
            table.AddColumn(new TableColumn("[bold dim]Current Trick[/]").Centered());

            foreach (var trickCard in _gameConsole.GameState.CurrentTrick)
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

        private string GetCardDisplay(Card card)
        {
            if (card.IsJoker) return "[bold magenta]🃏 Joker[/]";

            var suitColor = card.Suit == Suit.Hearts || card.Suit == Suit.Diamonds ? "red" : "blue";
            var suitSymbol = GetSuitDisplay(card.Suit);
            var rankStr = card.Rank.ToString();

            return $"[{suitColor}]{suitSymbol}{rankStr}[/]";
        }

        private string GetSuitDisplay(Suit suit)
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

        private void ResetForNewRound()
        {
            foreach (var player in _gameConsole.GetPlayers())
            {
                player.Hand.Clear();
                player.DeclaredMelds?.Clear();
                player.IsDealer = !player.IsDealer;
            }

            var newCurrent = _gameConsole.GetPlayers().FirstOrDefault(p => !p.IsDealer);
            if (newCurrent != null)
            {
                _gameConsole.SetCurrentPlayer(_gameConsole.GetPlayers().IndexOf(newCurrent));
            }

            _gameConsole.GameState.StartNewTrick();

            // Deal cards for new round
            _gameConsole.DealCards();
            _stateMachine.DispatchCardsDealt();
            _gameConsole.FlipTrumpCard();
            _stateMachine.DispatchTrumpDetermined();
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

        private class PossibleMeld
        {
            public MeldType Type { get; set; }
            public int Points { get; set; }
            public List<Card> Cards { get; set; } = new();
        }
    }
}
