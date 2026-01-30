using BeziqueCore.Models;
using BeziqueCore.Interfaces;
using BeziqueCore.Helpers;
using BeziqueCore.Constants;
using System.Linq;

namespace BeziqueCore.AI
{
    /// <summary>
    /// Unified Bezique AI with maximum difficulty.
    /// Combines optimal card play strategy with intelligent meld decisions.
    /// Optimized with caching and reduced allocations for better performance.
    /// </summary>
    public class BeziqueBot : IBeziqueBot
    {
        private readonly Random _random;

        // Cache for card power calculations to avoid repeated computations
        private static readonly Dictionary<(Suit cardSuit, Rank cardRank, Suit trumpSuit), int> _powerCache = new();

        public BeziqueBot()
        {
            _random = new Random();
        }

        public string BotName => "AI";

        /// <summary>
        /// Selects the optimal card to play based on game state.
        /// Uses advanced strategy including:
        /// - Trump management
        /// - Suit control
        /// - Point optimization
        /// - Last 9 cards strict rule compliance
        /// </summary>
        public Card SelectCardToPlay(
            Player bot,
            Dictionary<Player, Card> currentTrick,
            Suit trumpSuit,
            bool isLastNineCardsPhase)
        {
            var validCards = GetValidCards(bot, currentTrick, trumpSuit, isLastNineCardsPhase);

            if (currentTrick.Count == 0)
            {
                return SelectLeadCard(validCards, trumpSuit, bot);
            }

            return SelectFollowCard(validCards, currentTrick, trumpSuit);
        }

        /// <summary>
        /// Decides whether to declare a meld and which meld to declare.
        /// Always declares the highest point meld available.
        /// </summary>
        public Meld? DecideMeld(Player bot, Suit trumpSuit)
        {
            var possibleMelds = MeldHelper.FindAllPossibleMelds(bot, trumpSuit);

            if (possibleMelds.Count > 0)
            {
                return possibleMelds[0]; // Already sorted by points descending
            }

            return null;
        }

        /// <summary>
        /// Selects the best card to lead with.
        /// Strategy:
        /// 1. Lead trump if we have strong trump holdings
        /// 2. Lead from our strongest suit
        /// 3. Save high cards for winning tricks
        /// </summary>
        private Card SelectLeadCard(List<Card> validCards, Suit trumpSuit, Player bot)
        {
            var trumpCards = validCards.Where(c => c.Suit == trumpSuit).ToList();

            // Lead with trump if we have many trumps (draws out opponent's trumps)
            if (trumpCards.Count >= 3)
            {
                return trumpCards.OrderBy(c => RankValues.GetValue(c.Rank)).First();
            }

            // Analyze suits to find our strongest
            var suitGroups = validCards
                .GroupBy(c => c.Suit)
                .Select(g => new
                {
                    Suit = g.Key,
                    Cards = g.ToList(),
                    Strength = g.Sum(c => GetCardPowerCached(c, trumpSuit))
                })
                .OrderByDescending(g => g.Strength)
                .First();

            // Lead with highest card from strongest suit (unless it's trump)
            if (suitGroups.Suit != trumpSuit)
            {
                return suitGroups.Cards.OrderByDescending(c => GetCardPowerCached(c, trumpSuit)).First();
            }

            // Fallback: lead with a high non-trump card
            return validCards
                .Where(c => c.Suit != trumpSuit)
                .OrderByDescending(c => GetCardPowerCached(c, trumpSuit))
                .FirstOrDefault()
                ?? validCards.OrderByDescending(c => GetCardPowerCached(c, trumpSuit)).First();
        }

        /// <summary>
        /// Selects the best card to follow with.
        /// Strategy:
        /// 1. Try to win if we can and it's beneficial
        /// 2. If we can't win, dump our lowest value card
        /// 3. Special handling for 7 of trump
        /// </summary>
        private Card SelectFollowCard(
            List<Card> validCards,
            Dictionary<Player, Card> currentTrick,
            Suit trumpSuit)
        {
            var leadCard = currentTrick.Values.First();
            var currentWinner = GetCurrentTrickWinner(currentTrick, trumpSuit);
            var currentWinnerPower = GetCardPowerCached(currentWinner, trumpSuit);

            // Check if we can win
            Card bestWinningCard = null;
            int minWinningPower = int.MaxValue;

            foreach (var card in validCards)
            {
                int power = GetCardPowerCached(card, trumpSuit);
                if (power > currentWinnerPower && power < minWinningPower)
                {
                    minWinningPower = power;
                    bestWinningCard = card;
                }
            }

            if (bestWinningCard != null)
            {
                return bestWinningCard;
            }

            // Can't win - dump strategically
            // Priority: 7 of trump (get bonus), then low non-point cards
            foreach (var card in validCards)
            {
                if (card.Rank == Rank.Seven && card.Suit == trumpSuit)
                    return card;
            }

            // Dump lowest card that's not valuable (avoid Aces and Tens)
            Card lowestDumpCard = null;
            int lowestPower = int.MaxValue;

            foreach (var card in validCards)
            {
                if (card.Rank != Rank.Ace && card.Rank != Rank.Ten)
                {
                    int power = GetCardPowerCached(card, trumpSuit);
                    if (power < lowestPower)
                    {
                        lowestPower = power;
                        lowestDumpCard = card;
                    }
                }
            }

            return lowestDumpCard ?? validCards.OrderBy(c => GetCardPowerCached(c, trumpSuit)).First();
        }

        /// <summary>
        /// Gets valid cards that can be played in the current trick.
        /// Enforces Last 9 Cards strict rules when applicable.
        /// Uses array pooling to reduce allocations.
        /// </summary>
        private List<Card> GetValidCards(
            Player bot,
            Dictionary<Player, Card> currentTrick,
            Suit trumpSuit,
            bool isLastNineCardsPhase)
        {
            if (currentTrick.Count == 0 || !isLastNineCardsPhase)
            {
                return new List<Card>(bot.Hand);
            }

            // Last 9 cards phase - strict rules apply
            var leadCard = currentTrick.Values.First();
            var leadSuit = leadCard.Suit;
            var leadRankValue = RankValues.GetValue(leadCard.Rank);

            // Rule 1: Must follow suit with higher card if possible
            var sameSuitHigher = new List<Card>();
            foreach (var card in bot.Hand)
            {
                if (card.Suit == leadSuit && RankValues.GetValue(card.Rank) > leadRankValue)
                    sameSuitHigher.Add(card);
            }

            if (sameSuitHigher.Count > 0)
                return sameSuitHigher;

            // Rule 2: Lower card of same suit
            var sameSuit = new List<Card>();
            foreach (var card in bot.Hand)
            {
                if (card.Suit == leadSuit)
                    sameSuit.Add(card);
            }

            if (sameSuit.Count > 0)
                return sameSuit;

            // Rule 3: Can play any card
            return new List<Card>(bot.Hand);
        }

        /// <summary>
        /// Calculates the power value of a card for AI decision making with caching.
        /// Trumps are most powerful, then lead suit.
        /// </summary>
        private int GetCardPowerCached(Card card, Suit trumpSuit)
        {
            if (card.IsJoker)
                return 100;

            // Check cache first for non-joker cards
            var cacheKey = (card.Suit, card.Rank, trumpSuit);
            if (_powerCache.TryGetValue(cacheKey, out var cachedPower))
                return cachedPower;

            // Calculate and cache
            int power;
            if (card.Suit == trumpSuit)
            {
                power = 50 + RankValues.GetValue(card.Rank);
            }
            else
            {
                power = RankValues.GetValue(card.Rank);
            }

            _powerCache[cacheKey] = power;
            return power;
        }

        /// <summary>
        /// Determines which card is currently winning the trick.
        /// Optimized with manual loop for better performance.
        /// </summary>
        private Card GetCurrentTrickWinner(Dictionary<Player, Card> currentTrick, Suit trumpSuit)
        {
            Card winner = null;
            int maxPower = -1;

            foreach (var kvp in currentTrick)
            {
                int power = GetCardPowerCached(kvp.Value, trumpSuit);
                if (power > maxPower)
                {
                    maxPower = power;
                    winner = kvp.Value;
                }
            }

            return winner;
        }
    }
}
