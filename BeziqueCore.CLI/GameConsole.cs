using BeziqueCore.Adapters;
using BeziqueCore.Actions;
using BeziqueCore.Deck;
using BeziqueCore.Notifiers;
using BeziqueCore.Validators;
using BeziqueCore.Resolvers;
using BeziqueCore.Timers;
using BeziqueCore.Models;
using BeziqueCore.Interfaces;
using Spectre.Console;

namespace BeziqueCore.CLI
{
    public class GameConsole : IGameAdapter
    {
        private readonly IDeckOperations _deckOps;
        private readonly IPlayerActions _playerActions;
        private readonly IGameStateNotifier _notifier;
        private readonly IGameState _gameState;
        private readonly IPlayerTimer _playerTimer;
        private readonly IMeldValidator _meldValidator;
        private readonly ITrickResolver _trickResolver;
        private readonly Dictionary<Player, List<Card>> _playedCards;
        private Suit _trumpSuit;
        private int _currentPlayerIndex;

        public GameConsole()
        {
            _deckOps = new DeckOperations();
            _meldValidator = new MeldValidator();
            _notifier = new GameNotifier();
            _playerTimer = new PlayerTimer();
            _gameState = new GameState(_playerTimer);
            _trickResolver = new TrickResolver(_gameState);
            _playerActions = new PlayerActions(_deckOps, _meldValidator, _notifier, _gameState);
            _playedCards = new Dictionary<Player, List<Card>>();
            _trumpSuit = default;
            _currentPlayerIndex = 0;
        }

        public void InitializeGame()
        {
            AnsiConsole.MarkupLine("[bold yellow]🎴 Initializing Bezique Card Game...[/]");
            _deckOps.InitializeDeck();
            _gameState.Reset();
            _playedCards.Clear();

            // Set non-dealer as first player to lead
            var nonDealer = _gameState.Players.FirstOrDefault(p => !p.IsDealer)
                         ?? _gameState.Players.FirstOrDefault();
            if (nonDealer != null)
            {
                _gameState.CurrentPlayer = nonDealer;
            }

            // Initialize first trick
            _gameState.StartNewTrick();
        }

        public void NotifyGameInitialized()
        {
            _notifier.NotifyGameStarted();
        }

        public void DealCards()
        {
            AnsiConsole.MarkupLine("[cyan]🎴 Dealing cards to players...[/]");
            var players = _gameState.Players;

            AnsiConsole.MarkupLine($"[dim]Player count: {players.Count}[/]");
            AnsiConsole.MarkupLine($"[dim]Before dealing - Deck: {_deckOps.GetRemainingCardCount()} cards[/]");

            for (int i = 0; i < 9; i += 3)
            {
                AnsiConsole.MarkupLine($"[dim]Outer loop iteration {i}[/]");
                foreach (var player in players)
                {
                    AnsiConsole.MarkupLine($"[dim]Dealing to {player.Name}[/]");
                    for (int j = 0; j < 3 && _deckOps.GetRemainingCardCount() > 1; j++)
                    {
                        AnsiConsole.MarkupLine($"[dim]  Inner loop iteration {j}, deck count: {_deckOps.GetRemainingCardCount()}[/]");
                        var card = _deckOps.DrawTopCard();
                        if (card != null)
                        {
                            player.Hand.Add(card);
                            AnsiConsole.MarkupLine($"[dim]Dealt {card.Rank} of {card.Suit} to {player.Name}[/]");
                        }
                    }
                }
            }

            AnsiConsole.MarkupLine($"[dim]After dealing - Deck: {_deckOps.GetRemainingCardCount()} cards[/]");

            DisplayHands();
        }

        public void NotifyCardsDealt()
        {
            _notifier.NotifyCardsDealt(_gameState.Players.ToDictionary(p => p, p => p.Hand));
        }

        public void FlipTrumpCard()
        {
            AnsiConsole.MarkupLine("[yellow]🃏 Flipping trump card...[/]");
            var trumpCard = _deckOps.FlipTrumpCard();
            if (trumpCard == null)
            {
                throw new InvalidOperationException("No cards remaining in deck");
            }

            _gameState.TrumpSuit = trumpCard.Suit;
            _gameState.TrumpCard = trumpCard;
            _trumpSuit = trumpCard.Suit;

            var suitIcon = GetSuitIcon(trumpCard.Suit);
            AnsiConsole.MarkupLine($"[bold green]Trump: {suitIcon} {trumpCard.Rank}[/]");

            if (trumpCard.Rank == Rank.Seven)
            {
                var dealer = _gameState.Players.FirstOrDefault(p => p.IsDealer);
                if (dealer != null)
                {
                    dealer.Score += 10;
                    AnsiConsole.MarkupLine($"[green]✨ Dealer gets 10 points for 7 of Trump![/]");
                }
            }
        }

        public void NotifyTrumpDetermined()
        {
            _notifier.NotifyTrumpDetermined(_gameState.TrumpSuit, _gameState.TrumpCard);
        }

        public void StartPlayerTimer()
        {
            _playerTimer.Start();
            var current = _gameState.CurrentPlayer;
            AnsiConsole.MarkupLine($"[bold blue]⏱️ {current?.Name}'s turn (15 seconds)[/]");
        }

        public void StopPlayerTimer()
        {
            _playerTimer.Stop();
        }

        public void ResetPlayerTimer()
        {
            _playerTimer.Reset();
        }

        public void DeductTimeoutPoints()
        {
            var current = _gameState.CurrentPlayer;
            if (current != null)
            {
                current.Score = Math.Max(0, current.Score - 10);
                AnsiConsole.MarkupLine($"[red]⏰ {current.Name} timed out! -10 points[/]");
                _notifier.NotifyPlayerTimeout(current);
            }
        }

        public void CalculateRoundScores()
        {
            AnsiConsole.MarkupLine("[cyan]📊 Calculating round scores...[/]");
            foreach (var player in _gameState.Players)
            {
                _gameState.RoundScores[player] = player.Score;
            }
        }

        public void NotifyRoundEnded()
        {
            _notifier.NotifyRoundEnded(_gameState.RoundScores);
        }

        public void DeclareWinner()
        {
            var winner = _gameState.Players.OrderByDescending(p => p.Score).First();
            _gameState.Winner = winner;
        }

        public void NotifyGameOver()
        {
            _notifier.NotifyGameOver(_gameState.Winner);
        }

        public bool IsLastNineCardsPhase()
        {
            // Last 9 cards phase is reached when deck is empty (0 cards remaining)
            // This is checked AFTER drawing cards
            return _deckOps.GetRemainingCardCount() == 0;
        }

        public bool IsDeckEmpty()
        {
            return _deckOps.GetRemainingCardCount() == 0;
        }

        public bool AreAllHandsEmpty()
        {
            return _gameState.Players.All(p => p.Hand.Count == 0);
        }

        // Gameplay actions
        public void ProcessOpponentResponses()
        {
            // In single-player CLI, this is a no-op
            // Would wait for network opponents in multiplayer
        }

        public void ResolveTrick()
        {
            var currentTrick = _gameState.CurrentTrick;
            if (currentTrick == null || currentTrick.Count == 0)
            {
                return;
            }

            // Get lead suit (handle joker case - if joker led, use trump as default)
            Suit leadSuit = _gameState.LeadSuit ?? _gameState.TrumpSuit;

            // Determine winner
            var winner = _trickResolver.DetermineTrickWinner(
                currentTrick,
                _gameState.TrumpSuit,
                leadSuit
            );

            // Calculate points
            var cards = currentTrick.Values.ToList();
            var points = _trickResolver.CalculateTrickPoints(cards);

            // Award points
            winner.Score += points;

            // Store winner for drawing phase
            _gameState.LastTrickWinner = winner;

            // Notify
            _notifier.NotifyTrickWon(winner, cards.ToArray(), points);

            // Winner leads next trick
            _gameState.CurrentPlayer = winner;

            // Clear trick for next round
            _gameState.StartNewTrick();
        }

        public void ProcessMeldOpportunity()
        {
            // Meld opportunity is handled by the menu system
        }

        public void ScoreMeld()
        {
            // Meld scoring is handled by IPlayerActions.DeclareMeld
        }

        public void DrawCards()
        {
            // Winner draws first (top card), then loser draws next
            var winner = _gameState.LastTrickWinner;
            if (winner == null)
            {
                return;
            }

            // Winner draws first
            if (_deckOps.GetRemainingCardCount() > 0)
            {
                var card = _deckOps.DrawTopCard();
                if (card != null)
                {
                    winner.Hand.Add(card);
                    AnsiConsole.MarkupLine($"[dim]{winner.Name} drew {GetCardString(card)}[/]");
                }
            }

            // Find the loser (the other player)
            var loser = _gameState.Players.FirstOrDefault(p => p != winner);
            if (loser != null && _deckOps.GetRemainingCardCount() > 0)
            {
                var card = _deckOps.DrawTopCard();
                if (card != null)
                {
                    loser.Hand.Add(card);
                    AnsiConsole.MarkupLine($"[dim]{loser.Name} drew {GetCardString(card)}[/]");
                }
            }
        }

        public void CheckDeck()
        {
            // Check if we should transition to LAST_9_CARDS phase
            if (IsLastNineCardsPhase())
            {
                AnsiConsole.MarkupLine("[bold yellow]⚠️ Last 9 Cards phase reached![/]");
            }
        }

        // Last 9 cards actions
        public void ProcessL9OpponentResponses()
        {
            ProcessOpponentResponses();
        }

        public void ResolveL9Trick()
        {
            ResolveTrick();
        }

        public void CheckL9TrickComplete()
        {
            // Check if we should continue or end the round
            // This is handled by the state machine helper
        }

        public void CalculateL9FinalScores()
        {
            AnsiConsole.MarkupLine("[cyan]📊 Calculating final scores (Last 9 Cards)...[/]");
            foreach (var player in _gameState.Players)
            {
                _gameState.RoundScores[player] = player.Score;
            }
        }

        public BeziqueGameFlow CreateStateMachine()
        {
            return new BeziqueGameFlow(this);
        }

        public IDeckOperations DeckOps => _deckOps;
        public IPlayerActions PlayerActions => _playerActions;
        public IGameStateNotifier Notifier => _notifier;
        public IGameState GameState => _gameState;
        public IMeldValidator MeldValidator => _meldValidator;
        public ITrickResolver TrickResolver => _trickResolver;

        public void DisplayHands()
        {
            var table = new Table();
            table.Border(TableBorder.Rounded);
            table.AddColumn("[bold]Player[/]");
            table.AddColumn("[bold]Hand[/]");
            table.AddColumn("[bold]Score[/]");

            foreach (var player in _gameState.Players)
            {
                var hand = string.Join(", ", player.Hand.Select(c => GetCardString(c)));
                table.AddRow(
                    player.Name,
                    hand,
                    player.Score.ToString()
                );
            }

            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"[dim]Deck: {_deckOps.GetRemainingCardCount()} cards remaining[/]");
        }

        public void DisplayGameState()
        {
            var panel = new Panel($@"
Current State: [bold blue]{GetCurrentStateName()}[/]
Current Player: [bold]{_gameState.CurrentPlayer?.Name ?? "None"}[/]
Trump: {GetSuitIcon(_trumpSuit)} {_trumpSuit}
Deck: {_deckOps.GetRemainingCardCount()} cards
Last 9 Phase: {(IsLastNineCardsPhase() ? "[bold red]YES[/]" : "[green]NO[/]")}");

            panel.Header = new PanelHeader("[bold yellow]🎮 Game State[/]");
            panel.Border = BoxBorder.Rounded;
            AnsiConsole.Write(panel);
        }

        private string GetCurrentStateName()
        {
            return _currentPlayerIndex.ToString();
        }

        private string GetCardString(Card card)
        {
            if (card.IsJoker) return "🃏";
            return $"{GetSuitIcon(card.Suit)}{GetRankSymbol(card.Rank)}";
        }

        private string GetSuitIcon(Suit suit)
        {
            return suit switch
            {
                Suit.Hearts => "♥",
                Suit.Diamonds => "♦",
                Suit.Clubs => "♣",
                Suit.Spades => "♠",
                _ => "?"
            };
        }

        private string GetRankSymbol(Rank rank)
        {
            return rank switch
            {
                Rank.Ace => "A",
                Rank.King => "K",
                Rank.Queen => "Q",
                Rank.Jack => "J",
                Rank.Ten => "10",
                Rank.Nine => "9",
                Rank.Eight => "8",
                Rank.Seven => "7",
                _ => rank.ToString()
            };
        }

        public void SetCurrentPlayer(int index)
        {
            if (index >= 0 && index < _gameState.Players.Count)
            {
                _currentPlayerIndex = index;
                _gameState.CurrentPlayer = _gameState.Players[index];
            }
        }

        public Player GetCurrentPlayer()
        {
            return _gameState.CurrentPlayer;
        }

        public List<Player> GetPlayers()
        {
            return _gameState.Players;
        }

        public void AddPlayer(Player player)
        {
            _gameState.AddPlayer(player);
        }
    }
}
