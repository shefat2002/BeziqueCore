using BeziqueCore.Interfaces;
using BeziqueCore.Models;

namespace BeziqueCore.Actions
{
    public class PlayerActions : IPlayerActions
    {
        private readonly IDeckOperations _deckOps;
        private readonly IMeldValidator _meldValidator;
        private readonly IGameStateNotifier _notifier;
        private readonly IGameState _gameState;
        private const int SevenOfTrumpBonus = 10;

        public PlayerActions(
            IDeckOperations deckOps,
            IMeldValidator meldValidator,
            IGameStateNotifier notifier,
            IGameState gameState)
        {
            _deckOps = deckOps;
            _meldValidator = meldValidator;
            _notifier = notifier;
            _gameState = gameState;
        }

        public void PlayCard(Player player, Card card)
        {
            if (player.Hand == null)
            {
                throw new InvalidOperationException("Player hand is null.");
            }

            // Find the actual card in hand (using value equality)
            var cardInHand = player.Hand.FirstOrDefault(c => c.Equals(card));
            if (cardInHand == null)
            {
                throw new InvalidOperationException("Player does not have this card in hand.");
            }

            // Remove the actual card reference from hand
            player.Hand.Remove(cardInHand);

            // Check if playing 7 of trump - award 10 points
            if (card.Rank == Rank.Seven && card.Suit == _gameState.TrumpSuit)
            {
                player.Score += SevenOfTrumpBonus;
                _notifier.NotifySevenOfTrumpPlayed(player);
            }

            // Add to current trick
            _gameState.AddCardToTrick(player, cardInHand);

            _notifier.NotifyCardPlayed(player, cardInHand);

            // Check if trick is complete (all players have played)
            if (_gameState.IsTrickComplete())
            {
                _notifier.NotifyTrickComplete(_gameState.CurrentTrick);
            }
        }

        public void DeclareMeld(Player player, Meld meld)
        {
            if (!_meldValidator.IsValidMeld(meld.Cards.ToArray(), _gameState.TrumpSuit))
            {
                throw new InvalidOperationException("Invalid meld.");
            }

            // According to Bezique rules: at least one card in the meld must be unmelded
            // This allows reusing previously melded cards as long as there's at least one new card
            if (player.MeldedCards != null && player.MeldedCards.Count > 0)
            {
                // Count how many cards in this meld are already melded
                int alreadyMeldedCount = 0;
                foreach (var card in meld.Cards)
                {
                    if (player.MeldedCards.Any(mc => mc.Equals(card)))
                    {
                        alreadyMeldedCount++;
                    }
                }

                // If ALL cards are already melded, this is invalid
                // At least one card must be new (unmelded)
                if (alreadyMeldedCount == meld.Cards.Count)
                {
                    throw new InvalidOperationException("All cards in this meld have already been used. At least one card must be unmelded.");
                }
            }

            // Check for duplicate meld declaration (prevents declaring the EXACT same meld twice)
            if (player.DeclaredMelds != null)
            {
                foreach (var declaredMeld in player.DeclaredMelds)
                {
                    var declaredCards = declaredMeld.Cards;
                    var newCards = meld.Cards;

                    // Check if it's the exact same meld (same cards in same combination)
                    if (declaredCards.Count == newCards.Count &&
                        declaredCards.All(newCards.Contains))
                    {
                        throw new InvalidOperationException("This exact meld has already been declared.");
                    }
                }
            }

            meld.Points = _meldValidator.CalculateMeldPoints(meld);
            player.Score += meld.Points;

            // Store the declared meld (cards stay in hand!)
            if (player.DeclaredMelds == null)
            {
                player.DeclaredMelds = new List<Meld>();
            }
            player.DeclaredMelds.Add(meld);

            // Track all melded cards so we know which cards have been used
            if (player.MeldedCards == null)
            {
                player.MeldedCards = new List<Card>();
            }
            foreach (var card in meld.Cards)
            {
                // Only add if not already tracked (prevents duplicates)
                if (!player.MeldedCards.Any(mc => mc.Equals(card)))
                {
                    player.MeldedCards.Add(card);
                }
            }

            _notifier.NotifyMeldDeclared(player, meld, meld.Points);
        }

        public void DrawCard(Player player)
        {
            var card = _deckOps.DrawTopCard();
            if (card != null)
            {
                player.Hand.Add(card);
            }
        }

        public void SwitchSevenOfTrump(Player player)
        {
            var sevenOfTrump = player.Hand.FirstOrDefault(c =>
                c.Rank == Rank.Seven && c.Suit == _gameState.TrumpSuit);

            if (sevenOfTrump == null)
            {
                throw new InvalidOperationException("Player does not have 7 of trump.");
            }

            player.Score += SevenOfTrumpBonus;

            // Remove the actual card reference from hand
            player.Hand.Remove(sevenOfTrump);

            if (_gameState.TrumpCard != null)
            {
                player.Hand.Add(_gameState.TrumpCard);
                _gameState.TrumpCard = sevenOfTrump;
            }

            _notifier.NotifySevenOfTrumpSwitched(player);
        }

        public void SkipMeld(Player player)
        {
        }
    }
}
