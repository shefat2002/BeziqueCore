using BeziqueCore.Models;

namespace BeziqueCore.Interfaces
{
    public interface IMeldValidator
    {
        bool IsValidMeld(Card[] cards, Suit trumpSuit);
        int CalculateMeldPoints(Meld meld);
        MeldType DetermineMeldType(Card[] cards, Suit trumpSuit);
    }
}
