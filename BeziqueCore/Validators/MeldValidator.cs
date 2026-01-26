using BeziqueCore.Interfaces;
using BeziqueCore.Models;

namespace BeziqueCore.Validators
{
    public class MeldValidator : IMeldValidator
    {
        private const int TrumpRunPoints = 250;
        private const int TrumpSevenPoints = 10;
        private const int TrumpMarriagePoints = 40;
        private const int MarriagePoints = 20;
        private const int FourAcesPoints = 100;
        private const int FourKingsPoints = 80;
        private const int FourQueensPoints = 60;
        private const int FourJacksPoints = 40;
        private const int BeziquePoints = 40;
        private const int DoubleBeziquePoints = 500;
        private const int FourOfAKindCount = 4;
        private const int TrumpRunCardCount = 5;
        private const int MarriageCardCount = 2;
        private const int SingleCardCount = 1;
        private const int DoubleBeziqueCardCount = 4;

        public bool IsValidMeld(Card[] cards, Suit trumpSuit)
        {
            if (cards == null || cards.Length == 0)
            {
                return false;
            }

            var meldType = DetermineMeldType(cards, trumpSuit);
            return meldType != MeldType.InvalidMeld;
        }

        public int CalculateMeldPoints(Meld meld)
        {
            return meld.Type switch
            {
                MeldType.TrumpRun => TrumpRunPoints,
                MeldType.TrumpSeven => TrumpSevenPoints,
                MeldType.TrumpMarriage => TrumpMarriagePoints,
                MeldType.Marriage => MarriagePoints,
                MeldType.FourAces => FourAcesPoints,
                MeldType.FourKings => FourKingsPoints,
                MeldType.FourQueens => FourQueensPoints,
                MeldType.FourJacks => FourJacksPoints,
                MeldType.Bezique => BeziquePoints,
                MeldType.DoubleBezique => DoubleBeziquePoints,
                _ => 0
            };
        }

        public bool CanPlaceSequence(Card[] cards, List<Meld> meldHistory, Suit trumpSuit)
        {
            bool hasTrumpMarriage = meldHistory.Any(m => m.Type == MeldType.TrumpMarriage);

            if (cards.Length == TrumpRunCardCount)
            {
                return true;
            }

            if (!hasTrumpMarriage)
            {
                return false;
            }

            bool aceOfTrumpScored = meldHistory.Any(m =>
                m.Type == MeldType.FourAces &&
                m.Cards.Any(c => c.Suit == trumpSuit && c.Rank == Rank.Ace));

            if (aceOfTrumpScored)
            {
            }

            return true;
        }

        public MeldType DetermineMeldType(Card[] cards, Suit trumpSuit)
        {
            if (cards == null || cards.Length == 0)
            {
                return MeldType.InvalidMeld;
            }

            if (cards.Length == FourOfAKindCount)
            {
                if (cards.All(c => c.Rank == Rank.Ace))
                {
                    return MeldType.FourAces;
                }
                if (cards.All(c => c.Rank == Rank.King))
                {
                    return MeldType.FourKings;
                }
                if (cards.All(c => c.Rank == Rank.Queen))
                {
                    return MeldType.FourQueens;
                }
                if (cards.All(c => c.Rank == Rank.Jack))
                {
                    return MeldType.FourJacks;
                }

                int spadeQueens = cards.Count(c => c.Suit == Suit.Spades && c.Rank == Rank.Queen);
                int diamondJacks = cards.Count(c => c.Suit == Suit.Diamonds && c.Rank == Rank.Jack);

                if (spadeQueens == 2 && diamondJacks == 2)
                {
                    return MeldType.DoubleBezique;
                }
            }

            if (cards.Length == TrumpRunCardCount &&
                cards.All(c => c.Suit == trumpSuit) &&
                cards.Any(c => c.Rank == Rank.Ace) &&
                cards.Any(c => c.Rank == Rank.Ten) &&
                cards.Any(c => c.Rank == Rank.King) &&
                cards.Any(c => c.Rank == Rank.Queen) &&
                cards.Any(c => c.Rank == Rank.Jack))
            {
                return MeldType.TrumpRun;
            }

            if (cards.Length == MarriageCardCount &&
                cards.All(c => c.Suit == trumpSuit) &&
                cards.Any(c => c.Rank == Rank.King) &&
                cards.Any(c => c.Rank == Rank.Queen))
            {
                return MeldType.TrumpMarriage;
            }

            if (cards.Length == MarriageCardCount &&
                !cards.Any(c => c.Suit == trumpSuit) &&
                cards[0].Suit == cards[1].Suit &&
                cards.Any(c => c.Rank == Rank.King) &&
                cards.Any(c => c.Rank == Rank.Queen))
            {
                return MeldType.Marriage;
            }

            if (cards.Length == SingleCardCount &&
                cards[0].Suit == trumpSuit &&
                cards[0].Rank == Rank.Seven)
            {
                return MeldType.TrumpSeven;
            }

            if (cards.Length == MarriageCardCount &&
                cards.Any(c => c.Suit == Suit.Spades && c.Rank == Rank.Queen) &&
                cards.Any(c => c.Suit == Suit.Diamonds && c.Rank == Rank.Jack))
            {
                return MeldType.Bezique;
            }

            return MeldType.InvalidMeld;
        }
    }
}
