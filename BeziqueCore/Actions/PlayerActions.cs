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
            if (player.Hand == null || !player.Hand.Contains(card))
            {
                throw new InvalidOperationException("Player does not have this card in hand.");
            }

            player.Hand.Remove(card);

            // Check if playing 7 of trump - award 10 points
            if (card.Rank == Rank.Seven && card.Suit == _gameState.TrumpSuit)
            {
                player.Score += SevenOfTrumpBonus;
                _notifier.NotifySevenOfTrumpPlayed(player);
            }

            // Add to current trick
            _gameState.AddCardToTrick(player, card);

            _notifier.NotifyCardPlayed(player, card);

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

            // Check for duplicate meld declaration
            if (player.DeclaredMelds != null)
            {
                foreach (var declaredMeld in player.DeclaredMelds)
                {
                    if (declaredMeld.Type == meld.Type)
                    {
                        // Check if the same cards are being used (prevents declaring same meld twice)
                        var declaredCards = declaredMeld.Cards;
                        var newCards = meld.Cards;

                        if (declaredCards.All(newCards.Contains) && declaredCards.Count == newCards.Count)
                        {
                            throw new InvalidOperationException("This meld has already been declared.");
                        }
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
