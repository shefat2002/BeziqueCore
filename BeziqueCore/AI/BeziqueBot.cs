using BeziqueCore.Models;
using BeziqueCore.Interfaces;
using BeziqueCore.Helpers;
using System.Linq;

namespace BeziqueCore.AI
{
    /// <summary>
    /// Unified Bezique AI with maximum difficulty.
    /// Combines optimal card play strategy with intelligent meld decisions.
    /// </summary>
    public class BeziqueBot : IBeziqueBot
    {
        private readonly Random _random;

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
                // Leading - strategic choices
                return SelectLeadCard(validCards, trumpSuit, bot);
            }

            // Following - calculate optimal play
            return SelectFollowCard(validCards, currentTrick, trumpSuit);
        }

        /// <summary>
        /// Decides whether to declare a meld and which meld to declare.
        /// Always declares the highest point meld available.
        /// </summary>
        public Meld? DecideMeld(Player bot, Suit trumpSuit)
        {
            // Find all possible melds and return highest point one
            var possibleMelds = MeldHelper.FindAllPossibleMelds(bot, trumpSuit);

            if (possibleMelds.Any())
            {
                // Always declare the best meld available
                return possibleMelds.First(); // Already sorted by points descending
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
                // Lead with a low trump to force opponent to waste high trumps
                return trumpCards.OrderBy(c => GetRankValue(c.Rank)).First();
            }

            // Analyze suits to find our strongest
            var suitGroups = validCards
                .GroupBy(c => c.Suit)
                .Select(g => new
                {
                    Suit = g.Key,
                    Cards = g.ToList(),
                    Strength = g.Sum(c => GetCardPower(c, trumpSuit))
                })
                .OrderByDescending(g => g.Strength)
                .First();

            // Lead with highest card from strongest suit (unless it's trump)
            if (suitGroups.Suit != trumpSuit)
            {
                return suitGroups.Cards.OrderByDescending(c => GetCardPower(c, trumpSuit)).First();
            }

            // Fallback: lead with a high non-trump card
            return validCards
                .Where(c => c.Suit != trumpSuit)
                .OrderByDescending(c => GetCardPower(c, trumpSuit))
                .FirstOrDefault()
                ?? validCards.OrderByDescending(c => GetCardPower(c, trumpSuit)).First();
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
            var currentWinnerPower = GetCardPower(currentWinner, trumpSuit);

            // Check if we can win
            var winningCards = validCards
                .Where(c => GetCardPower(c, trumpSuit) > currentWinnerPower)
                .OrderBy(c => GetCardPower(c, trumpSuit))
                .ToList();

            if (winningCards.Any())
            {
                // Win with lowest card that wins (saves high cards)
                return winningCards.First();
            }

            // Can't win - dump strategically
            // Priority: 7 of trump (get bonus), then low non-point cards
            var sevenOfTrump = validCards.FirstOrDefault(c =>
                c.Rank == Rank.Seven && c.Suit == trumpSuit);

            if (sevenOfTrump != null)
                return sevenOfTrump;

            // Dump lowest card that's not valuable (avoid Aces and Tens)
            return validCards
                .Where(c => c.Rank != Rank.Ace && c.Rank != Rank.Ten)
                .OrderBy(c => GetCardPower(c, trumpSuit))
                .FirstOrDefault()
                ?? validCards.OrderBy(c => GetCardPower(c, trumpSuit)).First();
        }

        /// <summary>
        /// Gets valid cards that can be played in the current trick.
        /// Enforces Last 9 Cards strict rules when applicable.
        /// </summary>
        private List<Card> GetValidCards(
            Player bot,
            Dictionary<Player, Card> currentTrick,
            Suit trumpSuit,
            bool isLastNineCardsPhase)
        {
            if (currentTrick.Count == 0 || !isLastNineCardsPhase)
            {
                // Any card can be played
                return bot.Hand.ToList();
            }

            // Last 9 cards phase - strict rules apply
            var leadCard = currentTrick.Values.First();
            var leadSuit = leadCard.Suit;
            var leadRank = leadCard.Rank;

            // Rule 1: Must follow suit with higher card if possible
            var sameSuitHigher = bot.Hand
                .Where(c => c.Suit == leadSuit && GetRankValue(c.Rank) > GetRankValue(leadRank))
                .ToList();

            if (sameSuitHigher.Count > 0)
                return sameSuitHigher;

            // Rule 2: Lower card of same suit
            var sameSuit = bot.Hand.Where(c => c.Suit == leadSuit).ToList();
            if (sameSuit.Count > 0)
                return sameSuit;

            // Rule 3: Can play any card
            return bot.Hand.ToList();
        }

        /// <summary>
        /// Calculates the power value of a card for AI decision making.
        /// Trumps are most powerful, then lead suit.
        /// </summary>
        private int GetCardPower(Card card, Suit trumpSuit)
        {
            if (card.IsJoker)
                return 100;

            // Trump cards are most powerful
            if (card.Suit == trumpSuit)
                return 50 + GetRankValue(card.Rank);

            // Non-trump cards
            return GetRankValue(card.Rank);
        }

        /// <summary>
        /// Gets the numeric value of a card's rank for comparison.
        /// </summary>
        private int GetRankValue(Rank rank)
        {
            return rank switch
            {
                Rank.Ace => 14,
                Rank.Ten => 13,
                Rank.King => 12,
                Rank.Queen => 11,
                Rank.Jack => 10,
                Rank.Nine => 9,
                Rank.Eight => 8,
                Rank.Seven => 7,
                _ => 0
            };
        }

        /// <summary>
        /// Determines which card is currently winning the trick.
        /// </summary>
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
}
