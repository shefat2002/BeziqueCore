using BeziqueCore.Core.Domain.Entities;

namespace BeziqueCore.Core.Application.Interfaces
{
    public interface IDeckOperations
    {
        void InitializeDeck();
        Card? DrawTopCard();
        int GetRemainingCardCount();
        bool IsLastNineCards();
        Card? FlipTrumpCard();
        void ShuffleDeck();
        Card? GetTrumpCard();
        Card? TakeTrumpCard();
    }
}
