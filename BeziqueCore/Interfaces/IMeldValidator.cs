using BeziqueCore.Models;

namespace BeziqueCore.Interfaces
{
    public interface IMeldValidator
    {
        bool IsValidMeld(Card[] cards, Suit trumpSuit);
        int CalculateMeldPoints(Meld meld);
        bool CanPlaceSequence(Card[] cards, List<Meld> meldHistory, Suit trumpSuit);
        MeldType DetermineMeldType(Card[] cards, Suit trumpSuit);
    }
}
