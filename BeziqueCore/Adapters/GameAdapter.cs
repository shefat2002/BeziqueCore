using BeziqueCore.Interfaces;
using BeziqueCore.Models;

namespace BeziqueCore.Adapters
{
    public class GameAdapter : IGameAdapter
    {
        private readonly IDeckOperations _deckOps;
        private readonly IPlayerActions _playerActions;
        private readonly IGameStateNotifier _notifier;
        private readonly IGameState _gameState;
        private const int CardsPerPlayer = 9;
        private const int DealSetSize = 3;
        private const int TimeoutPenalty = 10;

        public GameAdapter(
            IDeckOperations deckOps,
            IPlayerActions playerActions,
            IGameStateNotifier notifier,
            IGameState gameState)
        {
            _deckOps = deckOps;
            _playerActions = playerActions;
            _notifier = notifier;
            _gameState = gameState;
        }

        public void InitializeGame()
        {
            _deckOps.InitializeDeck();
            _gameState.Reset();
        }

        public void NotifyGameInitialized()
        {
            _notifier.NotifyGameStarted();
        }

        public void DealCards()
        {
            var players = _gameState.Players;
            int totalCardsToDeal = players.Count * CardsPerPlayer;

            for (int i = 0; i < CardsPerPlayer; i += DealSetSize)
            {
                foreach (var player in players)
                {
                    for (int j = 0; j < DealSetSize && _deckOps.GetRemainingCardCount() > 1; j++)
                    {
                        var card = _deckOps.DrawTopCard();
                        if (card != null)
                        {
                            player.Hand.Add(card);
                            totalCardsToDeal--;
                        }
                    }
                }
            }
        }

        public void NotifyCardsDealt()
        {
            var hands = _gameState.Players.ToDictionary(p => p, p => p.Hand);
            _notifier.NotifyCardsDealt(hands);
        }

        public void FlipTrumpCard()
        {
            var trumpCard = _deckOps.FlipTrumpCard();
            if (trumpCard == null)
            {
                throw new InvalidOperationException("No cards remaining in deck");
            }

            _gameState.TrumpSuit = trumpCard.Suit;
            _gameState.TrumpCard = trumpCard;

            if (trumpCard.Rank == Rank.Seven && trumpCard.Suit == _gameState.TrumpSuit)
            {
                var dealer = _gameState.Players.FirstOrDefault(p => p.IsDealer);
                if (dealer != null)
                {
                    dealer.Score += 10;
                }
            }
        }

        public void NotifyTrumpDetermined()
        {
            _notifier.NotifyTrumpDetermined(_gameState.TrumpSuit, _gameState.TrumpCard);
        }

        public void StartPlayerTimer()
        {
            _gameState.CurrentPlayerTimer.Start();
        }

        public void StopPlayerTimer()
        {
            _gameState.CurrentPlayerTimer.Stop();
        }

        public void ResetPlayerTimer()
        {
            _gameState.CurrentPlayerTimer.Reset();
        }

        public void DeductTimeoutPoints()
        {
            var currentPlayer = _gameState.CurrentPlayer;
            if (currentPlayer != null)
            {
                currentPlayer.Score = Math.Max(0, currentPlayer.Score - TimeoutPenalty);
                _notifier.NotifyPlayerTimeout(currentPlayer);
            }
        }

        public void CalculateRoundScores()
        {
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

        // Gameplay actions
        public void ProcessOpponentResponses()
        {
            // Check if all opponents have responded to the played card
            // This is a no-op in single-player vs AI, but would wait for human opponents in multiplayer
        }

        public void ResolveTrick()
        {
            // Determine the winner of the current trick
            // The trick resolver logic is handled by IPlayerActions
        }

        public void ProcessMeldOpportunity()
        {
            // Give current player opportunity to declare a meld
            // This is handled by the CLI/UI asking the player
        }

        public void ScoreMeld()
        {
            // Award points for declared meld
            // Points are calculated by MeldValidator
        }

        public void DrawCards()
        {
            // Current player draws a card from the deck
            var currentPlayer = _gameState.CurrentPlayer;
            if (currentPlayer != null && _deckOps.GetRemainingCardCount() > 0)
            {
                var card = _deckOps.DrawTopCard();
                if (card != null)
                {
                    currentPlayer.Hand.Add(card);
                }
            }
        }

        public void CheckDeck()
        {
            // Check if we should transition to LAST_9_CARDS phase
            // This is handled by IsLastNineCardsPhase() method
        }

        // Last 9 cards actions
        public void ProcessL9OpponentResponses()
        {
            // Same as ProcessOpponentResponses but for last 9 cards phase
            ProcessOpponentResponses();
        }

        public void ResolveL9Trick()
        {
            // Same as ResolveTrick but for last 9 cards phase
            ResolveTrick();
        }

        public void CalculateL9FinalScores()
        {
            // Calculate final scores including any last 9 cards bonuses
            foreach (var player in _gameState.Players)
            {
                // Add any remaining card points
                // In last 9 cards, players get points for cards they've won
                _gameState.RoundScores[player] = player.Score;
            }
        }
    }
}
