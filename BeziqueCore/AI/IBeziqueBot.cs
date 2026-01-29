using BeziqueCore.Models;
using BeziqueCore.Interfaces;

namespace BeziqueCore.AI
{
    /// <summary>
    /// Interface for AI bot players in Bezique.
    /// Unity developers can implement this for custom AI behavior.
    /// </summary>
    public interface IBeziqueBot
    {
        /// <summary>
        /// Selects a card to play from the bot's hand.
        /// </summary>
        Card SelectCardToPlay(
            Player bot,
            Dictionary<Player, Card> currentTrick,
            Suit trumpSuit,
            bool isLastNineCardsPhase
        );

        /// <summary>
        /// Decides whether to declare a meld and which meld to declare.
        /// Returns null if no meld should be declared.
        /// </summary>
        Meld? DecideMeld(
            Player bot,
            Suit trumpSuit
        );

        /// <summary>
        /// Gets the bot's difficulty level/name.
        /// </summary>
        string BotName { get; }
    }

    /// <summary>
    /// Base class for Bezique AI bots with common functionality.
    /// </summary>
    public abstract class BeziqueBotBase : IBeziqueBot
    {
        protected readonly Random _random;

        protected BeziqueBotBase()
        {
            _random = new Random();
        }

        public abstract string BotName { get; }

        public abstract Card SelectCardToPlay(
            Player bot,
            Dictionary<Player, Card> currentTrick,
            Suit trumpSuit,
            bool isLastNineCardsPhase
        );

        public abstract Meld? DecideMeld(Player bot, Suit trumpSuit);

        /// <summary>
        /// Gets valid cards that can be played in the current trick.
        /// </summary>
        protected List<Card> GetValidCards(
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

            // Last 9 cards phase - strict rules
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
        /// </summary>
        protected int GetCardPower(Card card, Suit trumpSuit)
        {
            if (card.IsJoker)
                return 100;

            // Trump cards are most powerful
            if (card.Suit == trumpSuit)
                return 50 + GetRankValue(card.Rank);

            // Lead suit cards
            return GetRankValue(card.Rank);
        }

        protected int GetRankValue(Rank rank)
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
    }
}
