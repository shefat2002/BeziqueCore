using BeziqueCore.Core.Application.Interfaces;
using BeziqueCore.Core.Domain.Constants;
using BeziqueCore.Core.Domain.Entities;

namespace BeziqueCore.Core.Infrastructure.Validation
{
    public class TrickResolver : ITrickResolver
    {
        private readonly IGameState _gameState;

        public TrickResolver(IGameState gameState)
        {
            _gameState = gameState;
        }

        public Player DetermineTrickWinner(
            Dictionary<Player, Card> playedCards,
            Suit trumpSuit,
            Suit leadSuit)
        {
            if (playedCards == null || playedCards.Count == 0)
            {
                throw new ArgumentException("No cards were played.");
            }

            if (HasJoker(playedCards))
            {
                // Find any non-joker cards
                var nonJokerCards = playedCards
                    .Where(pc => !pc.Value.IsJoker)
                    .ToList();

                if (!nonJokerCards.Any())
                {
                    // All players played jokers - first player wins by default
                    return playedCards.First().Key;
                }

                // Check if a joker was played first
                var firstPlay = playedCards.First();
                if (firstPlay.Value.IsJoker)
                {
                    // Joker played first - only trump can win
                    var trumpCards = nonJokerCards
                        .Where(pc => pc.Value.Suit == trumpSuit)
                        .OrderByDescending(pc => (int)pc.Value.Rank)
                        .ToList();

                    if (trumpCards.Any())
                    {
                        // Highest trump wins
                        return trumpCards.First().Key;
                    }

                    // No trump played - the first non-joker card wins (joker has no value)
                    return nonJokerCards.First().Key;
                }
                else
                {
                    // Normal card played first, joker played later
                    // First player wins (joker has no value)
                    return firstPlay.Key;
                }
            }

            var trumpPlays = playedCards
                .Where(pc => pc.Value.Suit == trumpSuit)
                .OrderByDescending(pc => (int)pc.Value.Rank)
                .ToList();

            if (trumpPlays.Any())
            {
                return trumpPlays.First().Key;
            }

            var leadSuitPlays = playedCards
                .Where(pc => pc.Value.Suit == leadSuit)
                .OrderByDescending(pc => (int)pc.Value.Rank)
                .ToList();

            if (leadSuitPlays.Any())
            {
                return leadSuitPlays.First().Key;
            }

            return playedCards.First().Key;
        }

        public bool IsValidPlay(
            Card cardToPlay,
            List<Card> playerHand,
            Suit leadSuit,
            bool isLastNineCards)
        {
            if (cardToPlay == null || playerHand == null || !playerHand.Contains(cardToPlay))
            {
                return false;
            }

            // Jokers can always be played (they have no suit restrictions)
            if (cardToPlay.IsJoker)
            {
                return true;
            }

            if (isLastNineCards)
            {
                var higherSuitCards = playerHand
                    .Where(c => c.Suit == leadSuit && (int)c.Rank > (int)cardToPlay.Rank)
                    .ToList();

                if (higherSuitCards.Any())
                {
                    return false;
                }

                var sameSuitCards = playerHand.Where(c => c.Suit == leadSuit).ToList();
                if (sameSuitCards.Any() && cardToPlay.Suit != leadSuit)
                {
                    return false;
                }

                var trumpCards = playerHand.Where(c => c.Suit == _gameState.TrumpSuit).ToList();
                if (trumpCards.Any() && cardToPlay.Suit != _gameState.TrumpSuit)
                {
                    return false;
                }

                return true;
            }

            var leadSuitCards = playerHand.Where(c => c.Suit == leadSuit).ToList();
            if (leadSuitCards.Any() && cardToPlay.Suit != leadSuit)
            {
                return false;
            }

            return true;
        }

        public int CalculateTrickPoints(List<Card> cards)
        {
            int points = 0;

            foreach (var card in cards)
            {
                if (card.IsJoker)
                {
                    continue;
                }

                points += RankValues.GetTrickPoints(card.Rank);
            }

            return points;
        }

        public bool HasJoker(Dictionary<Player, Card> playedCards)
        {
            return playedCards.Any(pc => pc.Value.IsJoker);
        }

        public Suit GetLeadSuit(Dictionary<Player, Card> playedCards)
        {
            if (playedCards == null || playedCards.Count == 0)
            {
                throw new ArgumentException("No cards were played.");
            }

            var firstCard = playedCards.First().Value;

            if (firstCard.IsJoker)
            {
                var nextCard = playedCards.Skip(1).FirstOrDefault().Value;
                return nextCard?.Suit ?? _gameState.TrumpSuit;
            }

            return firstCard.Suit;
        }
    }
}
