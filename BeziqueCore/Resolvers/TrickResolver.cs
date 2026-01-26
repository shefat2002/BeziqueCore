using BeziqueCore.Interfaces;
using BeziqueCore.Models;

namespace BeziqueCore.Resolvers
{
    public class TrickResolver : ITrickResolver
    {
        private readonly IGameState _gameState;
        private const int AcePoints = 11;
        private const int TenPoints = 10;

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
                var firstPlay = playedCards.First();
                bool firstIsJoker = firstPlay.Value.IsJoker;

                if (firstIsJoker)
                {
                    var trumpCards = playedCards
                        .Where(pc => pc.Value.Suit == trumpSuit && !pc.Value.IsJoker)
                        .OrderByDescending(pc => (int)pc.Value.Rank)
                        .ToList();

                    if (trumpCards.Any())
                    {
                        return trumpCards.First().Key;
                    }

                    var secondPlay = playedCards.Skip(1).First();
                    return secondPlay.Key;
                }
                else
                {
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

                if (card.Rank == Rank.Ace)
                {
                    points += AcePoints;
                }
                else if (card.Rank == Rank.Ten)
                {
                    points += TenPoints;
                }
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
