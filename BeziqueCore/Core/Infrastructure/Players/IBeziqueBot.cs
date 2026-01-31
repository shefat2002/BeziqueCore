using BeziqueCore.Core.Domain.Entities;

namespace BeziqueCore.Core.Infrastructure.Players
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
