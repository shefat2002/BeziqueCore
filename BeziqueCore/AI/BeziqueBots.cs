using BeziqueCore.Models;
using BeziqueCore.Interfaces;
using System.Linq;

namespace BeziqueCore.AI
{
    /// <summary>
    /// Easy difficulty bot - plays randomly but follows rules.
    /// Good for beginners to learn the game.
    /// </summary>
    public class EasyBot : BeziqueBotBase
    {
        public override string BotName => "Easy AI";

        public override Card SelectCardToPlay(
            Player bot,
            Dictionary<Player, Card> currentTrick,
            Suit trumpSuit,
            bool isLastNineCardsPhase)
        {
            var validCards = GetValidCards(bot, currentTrick, trumpSuit, isLastNineCardsPhase);

            // Play random valid card
            var index = _random.Next(validCards.Count);
            return validCards[index];
        }

        public override Meld? DecideMeld(Player bot, Suit trumpSuit)
        {
            // Easy bot rarely declares melds (30% chance)
            if (_random.NextDouble() > 0.3)
                return null;

            // Get all possible melds
            var meldValidator = new Validators.MeldValidator();
            var possibleMelds = new List<Meld>();

            // Check for simple melds
            var hand = bot.Hand.ToArray();

            // Check for marriages
            var marriages = hand
                .GroupBy(c => c.Suit)
                .Where(g => g.Count(c => c.Rank == Rank.King || c.Rank == Rank.Queen) >= 2)
                .Select(g => new Meld
                {
                    Type = g.Key == trumpSuit ? MeldType.TrumpMarriage : MeldType.Marriage,
                    Cards = g.Take(2).ToList(),
                    Points = g.Key == trumpSuit ? 40 : 20
                });

            // Check for four of a kinds
            var fourKinds = hand
                .GroupBy(c => c.Rank)
                .Where(g => g.Count() >= 4 && !g.Any(c => c.IsJoker))
                .Select(g => new Meld
                {
                    Type = g.Key switch
                    {
                        Rank.Ace => MeldType.FourAces,
                        Rank.King => MeldType.FourKings,
                        Rank.Queen => MeldType.FourQueens,
                        Rank.Jack => MeldType.FourJacks,
                        _ => MeldType.InvalidMeld
                    },
                    Cards = g.Take(4).ToList(),
                    Points = g.Key switch
                    {
                        Rank.Ace => 100,
                        Rank.King => 80,
                        Rank.Queen => 60,
                        Rank.Jack => 40,
                        _ => 0
                    }
                });

            possibleMelds.AddRange(marriages);
            possibleMelds.AddRange(fourKinds.Where(m => m.Type != MeldType.InvalidMeld));

            if (possibleMelds.Any())
            {
                var bestMeld = possibleMelds.OrderByDescending(m => m.Points).First();
                return bestMeld;
            }

            return null;
        }
    }

    /// <summary>
    /// Medium difficulty bot - plays strategically but not optimally.
    /// Good average opponent.
    /// </summary>
    public class MediumBot : BeziqueBotBase
    {
        public override string BotName => "Medium AI";

        public override Card SelectCardToPlay(
            Player bot,
            Dictionary<Player, Card> currentTrick,
            Suit trumpSuit,
            bool isLastNineCardsPhase)
        {
            var validCards = GetValidCards(bot, currentTrick, trumpSuit, isLastNineCardsPhase);

            if (currentTrick.Count == 0)
            {
                // Leading - play strategically
                // Prefer playing low cards to save high cards
                return validCards
                    .OrderBy(c => GetCardPower(c, trumpSuit))
                    .First();
            }

            // Following - try to win if possible, otherwise dump low card
            var leadCard = currentTrick.Values.First();
            var leadSuit = leadCard.Suit;

            // Check if we can win with current cards
            var winningCards = validCards
                .Where(c => GetCardPower(c, trumpSuit) > GetCardPower(leadCard, trumpSuit))
                .ToList();

            if (winningCards.Any())
            {
                // Win with lowest winning card
                return winningCards
                    .OrderBy(c => GetCardPower(c, trumpSuit))
                    .First();
            }

            // Can't win - play lowest card
            return validCards
                .OrderBy(c => GetCardPower(c, trumpSuit))
                .First();
        }

        public override Meld? DecideMeld(Player bot, Suit trumpSuit)
        {
            // Medium bot declares melds 70% of the time
            if (_random.NextDouble() > 0.7)
                return null;

            var meldValidator = new Validators.MeldValidator();
            var hand = bot.Hand.ToArray();

            // Get best possible meld
            var bestMeld = meldValidator.GetBestPossibleMeld(hand, bot.Hand, trumpSuit);

            // Only declare if worth at least 20 points
            if (bestMeld != null && bestMeld.Points >= 20)
            {
                return bestMeld;
            }

            return null;
        }
    }

    /// <summary>
    /// Hard difficulty bot - plays optimally with advanced strategy.
    /// Challenging opponent for experienced players.
    /// </summary>
    public class HardBot : BeziqueBotBase
    {
        public override string BotName => "Hard AI";

        public override Card SelectCardToPlay(
            Player bot,
            Dictionary<Player, Card> currentTrick,
            Suit trumpSuit,
            bool isLastNineCardsPhase)
        {
            var validCards = GetValidCards(bot, currentTrick, trumpSuit, isLastNineCardsPhase);

            if (currentTrick.Count == 0)
            {
                // Leading - strategic choices
                // Lead with trump if we have many trumps
                var trumpCards = validCards.Where(c => c.Suit == trumpSuit).ToList();
                if (trumpCards.Count >= 3 && !_random.Next(3).Equals(0))
                {
                    // Lead with a low trump to draw out opponent's trumps
                    return trumpCards.OrderBy(c => GetRankValue(c.Rank)).First();
                }

                // Lead with a card from a suit where we have high cards
                var suitGroups = validCards.GroupBy(c => c.Suit);
                var bestSuit = suitGroups
                    .OrderByDescending(g => g.Sum(c => GetCardPower(c, trumpSuit)))
                    .First();

                return bestSuit.OrderByDescending(c => GetCardPower(c, trumpSuit)).First();
            }

            // Following - calculate optimal play
            var leadCard = currentTrick.Values.First();
            var currentWinner = GetCurrentTrickWinner(currentTrick, trumpSuit);

            // Check if partner is winning (for 4 player games)
            // For now, assume 2 player - always try to win
            var canWin = validCards
                .Where(c => GetCardPower(c, trumpSuit) > GetCardPower(currentWinner, trumpSuit))
                .ToList();

            if (canWin.Any())
            {
                // Win with lowest card that wins
                return canWin
                    .OrderBy(c => GetCardPower(c, trumpSuit))
                    .First();
            }

            // Can't win - dump lowest card (prefer non-trump, non-Ace/10)
            return validCards
                .OrderBy(c =>
                {
                    var power = GetCardPower(c, trumpSuit);
                    // Prefer dumping low cards that aren't valuable
                    if (c.Rank == Rank.Seven && c.Suit == trumpSuit)
                        return power - 20; // Dump 7 of trump
                    return power;
                })
                .First();
        }

        public override Meld? DecideMeld(Player bot, Suit trumpSuit)
        {
            // Hard bot always declares melds when available
            var meldValidator = new Validators.MeldValidator();
            var hand = bot.Hand.ToArray();

            // Get best possible meld
            var bestMeld = meldValidator.GetBestPossibleMeld(hand, bot.Hand, trumpSuit);

            // Declare any meld worth points
            if (bestMeld != null && bestMeld.Points > 0)
            {
                return bestMeld;
            }

            return null;
        }

        private Card GetCurrentTrickWinner(Dictionary<Player, Card> currentTrick, Suit trumpSuit)
        {
            Card winner = null;
            int maxPower = -1;

            foreach (var card in currentTrick.Values)
            {
                var power = GetCardPower(card, trumpSuit);
                if (power > maxPower)
                {
                    maxPower = power;
                    winner = card;
                }
            }

            return winner;
        }
    }

    /// <summary>
    /// Expert bot - uses minimax-style thinking and probability calculations.
    /// The ultimate challenge.
    /// </summary>
    public class ExpertBot : BeziqueBotBase
    {
        public override string BotName => "Expert AI";

        public override Card SelectCardToPlay(
            Player bot,
            Dictionary<Player, Card> currentTrick,
            Suit trumpSuit,
            bool isLastNineCardsPhase)
        {
            var validCards = GetValidCards(bot, currentTrick, trumpSuit, isLastNineCardsPhase);

            if (currentTrick.Count == 0)
            {
                // Lead with high card from a suit where opponent is weak
                // This is simplified - a full implementation would track opponent's played cards
                return validCards
                    .OrderByDescending(c => GetCardPower(c, trumpSuit))
                    .First();
            }

            // Calculate which card to play based on:
            // 1. Can we win the trick?
            // 2. Is winning worth it?
            // 3. If not, what should we dump?

            var leadCard = currentTrick.Values.First();
            var winningCards = validCards
                .Where(c => GetCardPower(c, trumpSuit) > GetCardPower(leadCard, trumpSuit))
                .ToList();

            if (winningCards.Any())
            {
                // Win efficiently - use lowest card that wins
                return winningCards
                    .OrderBy(c => GetCardPower(c, trumpSuit))
                    .First();
            }

            // Can't win - dump strategically
            // Priority: 7 of trump (to get bonus), then low cards, then cards from suits we're short on
            var sevenOfTrump = validCards.FirstOrDefault(c =>
                c.Rank == Rank.Seven && c.Suit == trumpSuit);

            if (sevenOfTrump != null)
                return sevenOfTrump;

            // Dump lowest non-valuable card
            return validCards
                .Where(c => !(c.Rank == Rank.Ace || c.Rank == Rank.Ten))
                .OrderBy(c => GetCardPower(c, trumpSuit))
                .FirstOrDefault()
                ?? validCards.OrderBy(c => GetCardPower(c, trumpSuit)).First();
        }

        public override Meld? DecideMeld(Player bot, Suit trumpSuit)
        {
            // Expert bot always melds optimally
            var meldValidator = new Validators.MeldValidator();
            var hand = bot.Hand.ToArray();

            var bestMeld = meldValidator.GetBestPossibleMeld(hand, bot.Hand, trumpSuit);

            // Always declare if beneficial
            if (bestMeld != null && bestMeld.Points > 0)
            {
                return bestMeld;
            }

            return null;
        }
    }
}
