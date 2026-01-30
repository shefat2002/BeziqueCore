using BeziqueCore.Interfaces;
using BeziqueCore.Models;
using BeziqueCore.Constants;

namespace BeziqueCore.Adapters
{
    public class GameAdapter : IGameAdapter
    {
        private readonly IDeckOperations _deckOps;
        private readonly IPlayerActions _playerActions;
        private readonly IGameStateNotifier _notifier;
        private readonly IGameState _gameState;
        private readonly ITrickResolver _trickResolver;

        public GameAdapter(
            IDeckOperations deckOps,
            IPlayerActions playerActions,
            IGameStateNotifier notifier,
            IGameState gameState,
            ITrickResolver trickResolver)
        {
            _deckOps = deckOps;
            _playerActions = playerActions;
            _notifier = notifier;
            _gameState = gameState;
            _trickResolver = trickResolver;
        }

        public void InitializeGame()
        {
            _deckOps.InitializeDeck();
            _gameState.Reset();

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
            var players = _gameState.Players;
            int totalCardsToDeal = players.Count * GameConstants.CardsPerPlayer;

            for (int i = 0; i < GameConstants.CardsPerPlayer; i += GameConstants.DealSetSize)
            {
                foreach (var player in players)
                {
                    for (int j = 0; j < GameConstants.DealSetSize && _deckOps.GetRemainingCardCount() > 1; j++)
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
                    dealer.Score += GameConstants.SevenOfTrumpBonus;
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
                currentPlayer.Score = Math.Max(0, currentPlayer.Score - GameConstants.TimeoutPenalty);
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

        public bool AreAllHandsEmpty()
        {
            return _gameState.Players.All(p => p.Hand.Count == 0);
        }

        // Gameplay actions
        public void ProcessOpponentResponses()
        {
            // Check if all opponents have responded to the played card
            // This is a no-op in single-player vs AI, but would wait for human opponents in multiplayer
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
            // Give current player opportunity to declare a meld
            // This is handled by the CLI/UI asking the player
            // The actual meld declaration happens through PlayerActions.DeclareMeld()
        }

        public void ScoreMeld()
        {
            // Award points for declared meld
            // Points are calculated by MeldValidator and added in PlayerActions.DeclareMeld()
            // This is just a notification point in the state machine
        }

        public bool IsTrickComplete()
        {
            return _gameState.CurrentTrick.Count >= _gameState.Players.Count;
        }

        public bool MorePlayersNeedToPlay()
        {
            return _gameState.CurrentTrick.Count < _gameState.Players.Count;
        }

        public void DrawCards()
        {
            // Winner draws first (top card), then loser draws next
            var winner = _gameState.LastTrickWinner;
            if (winner == null)
            {
                return;
            }

            // Find the loser (the other player)
            var loser = _gameState.Players.FirstOrDefault(p => p != winner);
            if (loser == null)
            {
                return;
            }

            // Special case: Last card before last 9 cards phase
            // For 2 players: 1 card face down + 1 trump face up = 2 cards remaining
            // For 4 players: 3 cards face down + 1 trump face up = 4 cards remaining
            int totalCardsRemaining = _deckOps.GetRemainingCardCount();
            int cardsToTakeFromDeck = _gameState.Players.Count == 2 ? 1 : 3;

            bool isLastTrickBeforeFinalPhase = totalCardsRemaining == (cardsToTakeFromDeck + 1);

            if (isLastTrickBeforeFinalPhase)
            {
                // Winner takes the last card(s) from deck
                for (int i = 0; i < cardsToTakeFromDeck && _deckOps.GetRemainingCardCount() > 1; i++)
                {
                    var card = _deckOps.DrawTopCard();
                    if (card != null)
                    {
                        winner.Hand.Add(card);
                    }
                }

                // Loser takes the flipped-up trump card
                var trumpCard = _deckOps.TakeTrumpCard();
                if (trumpCard != null)
                {
                    loser.Hand.Add(trumpCard);
                    _notifier.NotifyTrumpCardTaken(loser, trumpCard);
                }

                return;
            }

            // Normal drawing: Winner draws first
            if (_deckOps.GetRemainingCardCount() > 0)
            {
                var card = _deckOps.DrawTopCard();
                if (card != null)
                {
                    winner.Hand.Add(card);
                }
            }
            else if (_deckOps.GetTrumpCard() != null)
            {
                // Deck is empty, but trump card is available
                var trumpCard = _deckOps.TakeTrumpCard();
                if (trumpCard != null)
                {
                    winner.Hand.Add(trumpCard);
                    _notifier.NotifyTrumpCardTaken(winner, trumpCard);
                }
            }

            // Loser draws next
            if (_deckOps.GetRemainingCardCount() > 0)
            {
                var card = _deckOps.DrawTopCard();
                if (card != null)
                {
                    loser.Hand.Add(card);
                }
            }
            else if (_deckOps.GetTrumpCard() != null)
            {
                // Deck is empty, but trump card is available
                var trumpCard = _deckOps.TakeTrumpCard();
                if (trumpCard != null)
                {
                    loser.Hand.Add(trumpCard);
                    _notifier.NotifyTrumpCardTaken(loser, trumpCard);
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
            // Resolve the trick normally
            ResolveTrick();

            // Check if this is the last trick (all hands will be empty after this)
            bool allHandsEmpty = _gameState.Players.All(p => p.Hand.Count == 0);

            if (allHandsEmpty)
            {
                // Award bonus points for winning the last trick
                var winner = _gameState.LastTrickWinner;
                if (winner != null)
                {
                    winner.Score += GameConstants.LastTrickBonusPoints;
                    _notifier.NotifyLastTrickBonus(winner, GameConstants.LastTrickBonusPoints);
                }
            }
        }

        public void CheckL9TrickComplete()
        {
            // Check if we should continue or end the round
            // This is handled by AreAllHandsEmpty() method via the state machine helper
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

        public void CalculateAcesAndTens()
        {
            // ADVANCED MODE ONLY: Calculate additional points for Aces and Tens
            // This method is called by the ROUND_END state's 'do' action
            if (_gameState.Mode != GameMode.Advanced)
            {
                return;
            }

            foreach (var player in _gameState.Players)
            {
                int bonusPoints = 0;

                if (player.DeclaredMelds != null)
                {
                    foreach (var meld in player.DeclaredMelds)
                    {
                        foreach (var card in meld.Cards)
                        {
                            if (card.Rank == Rank.Ace || card.Rank == Rank.Ten)
                            {
                                bonusPoints += GameConstants.AceAndTenBonusPoints;
                            }
                        }
                    }
                }

                if (bonusPoints > 0)
                {
                    player.Score += bonusPoints;
                    _notifier.NotifyAcesAndTensBonus(player, bonusPoints);
                }
            }
        }
    }
}
