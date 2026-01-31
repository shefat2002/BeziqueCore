using BeziqueCore.Interfaces;
using BeziqueCore.Models;
using BeziqueCore.Constants;
using System.Linq;

namespace BeziqueCore.Validators
{
    public class MeldValidator : IMeldValidator
    {
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
            return MeldPoints.GetPointsForType(meld.Type);
        }

        public MeldType DetermineMeldType(Card[] cards, Suit trumpSuit)
        {
            if (cards == null || cards.Length == 0)
            {
                return MeldType.InvalidMeld;
            }

            if (cards.Length == FourOfAKindCount)
            {
                // Check for four-of-a-kind with Joker substitution
                // A Joker can substitute for any card in a four-of-a-kind
                var nonJokers = cards.Where(c => !c.IsJoker).ToList();
                var jokerCount = cards.Count(c => c.IsJoker);

                if (jokerCount > 0)
                {
                    // With jokers, check if non-jokers are all the same rank
                    if (nonJokers.All(c => c.Rank == Rank.Ace))
                    {
                        return MeldType.FourAces;
                    }
                    if (nonJokers.All(c => c.Rank == Rank.King))
                    {
                        return MeldType.FourKings;
                    }
                    if (nonJokers.All(c => c.Rank == Rank.Queen))
                    {
                        return MeldType.FourQueens;
                    }
                    if (nonJokers.All(c => c.Rank == Rank.Jack))
                    {
                        return MeldType.FourJacks;
                    }
                }
                else
                {
                    // No jokers - all must be same rank
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
                }

                int spadeQueens = cards.Count(c => !c.IsJoker && c.Suit == Suit.Spades && c.Rank == Rank.Queen);
                int diamondJacks = cards.Count(c => !c.IsJoker && c.Suit == Suit.Diamonds && c.Rank == Rank.Jack);

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
