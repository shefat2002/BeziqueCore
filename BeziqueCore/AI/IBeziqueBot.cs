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
}
