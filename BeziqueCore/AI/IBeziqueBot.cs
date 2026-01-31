using BeziqueCore.Models;
using BeziqueCore.Interfaces;

namespace BeziqueCore.AI
{
    public interface IBeziqueBot
    {
        Card SelectCardToPlay(
            Player bot,
            Dictionary<Player, Card> currentTrick,
            Suit trumpSuit,
            bool isLastNineCardsPhase
        );

        Meld? DecideMeld(
            Player bot,
            Suit trumpSuit
        );

        string BotName { get; }
    }
}
