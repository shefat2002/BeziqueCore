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

            meld.Points = _meldValidator.CalculateMeldPoints(meld);
            player.Score += meld.Points;
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
